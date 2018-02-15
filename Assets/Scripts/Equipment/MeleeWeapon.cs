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
	private Vector3 equipmentVelocity;

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
			playerStats.StaminaRemove (3f, true);
			// Audio
			if (attackSound != null) {
				StartCoroutine (HitAudioDelay ());
			}

			StartCoroutine (AttackCycle ());
			StartCoroutine (TrackEquipmentVelocity ());

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
					TakeDamage (hit.collider.name, livingEntity.entityGroupIndex, transform.name);
					if (playerStats != null) {
						playerStats.FatiqueRemove (.4f);
					}
					// Add force
					Vector3 meleeForce = equipmentVelocity * impactForce;
					CmdAddImpactForce (meleeForce, hit.point, hit.collider.name, livingEntity.entityGroupIndex);
				}

				// Add impact force
				Entity entity = hit.collider.GetComponent<Entity>();
				if (entity != null) {
					Vector3 meleeForce = equipmentVelocity * impactForce;
					CmdAddImpactForce (meleeForce, hit.point, hit.collider.name, entity.entityGroupIndex);
				}

				// Animation
				playerAnimationController.MeleeImpact();

				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}

				// Play impact audio
				if (entity != null) {
					AudioManager.instance.CmdPlayEntityImpactSound (entity.entitySoundMaterial.ToString(), this.weaponImpactSoundMaterial.ToString(), hit.point, "", 1f);
				} else {
					AudioClip genericClip = SoundLibrary.instance.GetEntityImpactSound ("Generic", weaponImpactSoundMaterial.ToString());
					if (genericClip != null) {
						AudioManager.instance.CmdPlaySound (genericClip.name, hit.point, "", 1);
					}
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

	IEnumerator TrackEquipmentVelocity() {
		Vector3 equipmentA = transform.position;
		Vector3 playerA = owner.transform.position;
		yield return new WaitForSeconds (.03f);
		Vector3 equipmentB = transform.position;
		Vector3 playerB = owner.transform.position;
		equipmentVelocity = (equipmentA-equipmentB);
		equipmentVelocity = equipmentVelocity.normalized;
		equipmentVelocity -= (playerA - playerB).normalized;
	}

	[Command]
	void CmdTakeDamage (string victimId, int targetGroup, string playerId) {
		if (GameManager.instance.GetLivingEntity (victimId, targetGroup) != null) {
			// Calculate Damage
			LivingEntity victim = GameManager.instance.GetLivingEntity (victimId, targetGroup);
			float dmg = CalculateDamage(victim.slashResistance, victim.bluntResistance, victim.piercingResistance, victim.professionWeakness, victim.weaknessAmount);
			victim.TakeDamage (dmg, playerId);
		}
	}

	public virtual void TakeDamage(string victimId, int targetGroup, string playerId) {
		CmdTakeDamage (victimId, targetGroup, playerId);
	}

	[Command]
	void CmdAddImpactForce(Vector3 meleeForce, Vector3 forcePoint, string targetName, int targetGroup) {

		LivingEntity targetLivingEntity = GameManager.instance.GetLivingEntity (targetName, targetGroup);
		if (targetLivingEntity != null) {
			targetLivingEntity.AddImpactForce (meleeForce, forcePoint);
		} else {
			Entity targetEntity = GameManager.instance.GetEntity (targetName, targetGroup);
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
		AudioManager.instance.CmdPlaySound2D (attackSound.name, transform.position, owner.name, 1);
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
