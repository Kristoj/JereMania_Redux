using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MeleeWeapon : Weapon {

	[Header ("Melee Properties")]
	public float meleeRange = 3f;
	public float hitDelay = .35f;
	public float audioDelay = .15f;
	public float impactForce = 5f;
	public AudioClip impactAudio;

	public override void ShootPrimary() {
		if (!weaponController.isChargingSecondaryAction && weaponController.canAttack) {
			StartCoroutine (StartAttack ());
		}
	}

	public virtual IEnumerator StartAttack() {
		if (fireMode == FireMode.Semi && !weaponController.mouseLeftReleased) {
			yield break;
		}
			
		if (Time.time - lastShotTime > timeBetweenShots) {
			playerAnimationController.Attack();
			lastShotTime = Time.time;
			weaponController.isAttacking = true;
			// Audio
			if (shootSound != null) {
				StartCoroutine (HitAudioDelay ());
			}

			StartCoroutine (AttackCycle ());

			yield return new WaitForSeconds (hitDelay);
			// Rot
			Vector3 tracerRot = new Vector3 (player.cam.transform.eulerAngles.x, player.cam.transform.eulerAngles.y, player.cam.transform.eulerAngles.z);

			// Raycast
			Ray ray = new Ray (player.cam.transform.position, Quaternion.Euler (tracerRot) * Vector3.forward);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, meleeRange, myHitMask, QueryTriggerInteraction.Collide)) {

				// Take damage
				LivingEntity livingEntity = hit.collider.GetComponent<LivingEntity>();
				if (livingEntity != null) {
					TakeDamage (hit.collider.name, transform.name);
					if (playerStats != null) {
						playerStats.RemoveFatique (.4f);
					}
				}

				// Add impact force
				Entity entity = hit.collider.GetComponent<Entity>();
				if (entity != null) {
					Vector3 meleeForce = Vector3.zero;
					if (playerAnimationController.lastAttackId == 0) {
						meleeForce = -player.transform.right;
						meleeForce += player.transform.forward * .15f;
						meleeForce *= impactForce;
					} else if (playerAnimationController.lastAttackId == 1) {
						meleeForce = player.transform.right;
						meleeForce += player.transform.forward * .15f;
						meleeForce *= impactForce;
					}
					CmdAddImpactForce (meleeForce, hit.point, hit.collider.name);
				}

				// Animation
				playerAnimationController.MeleeImpact();

				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}

				// Play impact audio
				if (entity != null) {
					AudioManager.instance.CmdPlayCustomSound2D (entity.impactSound.name, hit.point, "");
				} 
				else if (impactAudio != null) {
					AudioManager.instance.CmdPlayCustomSound2D (impactAudio.name, hit.point, "");
				}
				else {
					AudioManager.instance.CmdPlayCustomSound2D ("Impact_General", hit.point, "");
				}

				// Tell server to spawn tracer for all clients
				CmdOnProjectileHit (hit.point, transform.name, Quaternion.Euler (tracerRot), impactRot);
			}
		}
	}

	IEnumerator AttackCycle() {
		yield return new WaitForSeconds (60 / rpm);
		weaponController.isAttacking = false;
	}

	[Command]
	void CmdTakeDamage (string victimId, string playerId) {
		if (GameManager.GetCharacter (victimId) != null) {
			// Calculate Damage
			LivingEntity victim = GameManager.GetCharacter (victimId);
			float dmg = CalculateDamage(victim.slashResistance, victim.bluntResistance, victim.piercingResistance, victim.professionWeakness, victim.weaknessAmount);
			victim.TakeDamage (dmg, playerId);
		}
	}

	public virtual void TakeDamage(string victimId, string playerId) {
		CmdTakeDamage (victimId, playerId);
	}

	[Command]
	void CmdAddImpactForce(Vector3 meleeForce, Vector3 forcePoint, string targetName) {

		Entity targetLivingEntity = GameManager.GetCharacter (targetName);
		if (targetLivingEntity != null) {
			targetLivingEntity.AddImpactForce (meleeForce, forcePoint);
		} else {
			Entity targetEntity = GameManager.GetEntity (targetName);
			if (targetEntity != null) {
				targetEntity.AddImpactForce (meleeForce, forcePoint);
			}
		}
	}

	[Command]
	protected void CmdOnProjectileHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		RpcOnProjectileHit (hitPoint, id, tracerRot, impactRot);
	}

	[ClientRpc]
	protected void RpcOnProjectileHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		// Spawn impactFX
		if (impactFX != null) {
			Instantiate (impactFX, hitPoint, impactRot);
		}
	}

	IEnumerator HitAudioDelay() {
		yield return new WaitForSeconds (audioDelay);
		AudioManager.instance.CmdPlayCustomSound2D (shootSound.name, transform.position, owner.name);
	}

	public float CalculateDamage(float slashResistance, float bluntResistance, float piercingResistance, LivingEntity.ProfessionWeakness professionWeakness, float weaknessAmount) {
		// Calculate Base damage
		float baseDmgSummary = 0;
		baseDmgSummary += slashDamage - (slashDamage * (slashResistance / 100));
		baseDmgSummary += bluntDamage - (bluntDamage * (bluntResistance / 100));
		baseDmgSummary += piercingDamage - (piercingDamage * (piercingResistance / 100));

		// Calculate profession damage bonus
		float professionBonus = 0;
		// Woodcutting
		if (professionWeakness == LivingEntity.ProfessionWeakness.Woodcutting) {
			professionBonus = (weaknessAmount / 100) * (woodcuttingBonus / 100);
		}
		// Mining
		if (professionWeakness == LivingEntity.ProfessionWeakness.Mining) {
			professionBonus = (weaknessAmount / 100) * (miningBonus / 100);
		}
		// Harvesting
		if (professionWeakness == LivingEntity.ProfessionWeakness.Harvesting) {
			professionBonus = (weaknessAmount / 100) * (harvestingBonus / 100);
		}

		// Calculate final damage
		float finalDamage = baseDmgSummary + (baseDmgSummary * professionBonus);
		return finalDamage;
	}
}
