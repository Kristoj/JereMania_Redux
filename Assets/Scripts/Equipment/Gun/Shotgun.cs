using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shotgun : Gun {
	

	public override void Start() {
		base.Start ();
	}
		
	public override void ShootPrimary() {

		if (fireMode == FireMode.Semi && !weaponController.mouseLeftReleased) {
			return;
		}

		if (Time.time - lastShotTime > timeBetweenShots) {

			base.ShootPrimary ();
			playerAnimationController.Attack();

			for (int i = 0; i < projectileCount; i++) {

				float hipMultiplier = 1f;

				if (!weaponController.isChargingSecondaryAction) {
					hipMultiplier = hipSpreadMultiplier;
				}
				// Add recoil
				Vector3 tracerRot = new Vector3 (player.cam.transform.eulerAngles.x, player.cam.transform.eulerAngles.y, player.cam.transform.eulerAngles.z);
				tracerRot.x += Random.Range (-spread, spread) * hipMultiplier;
				tracerRot.y += Random.Range (-spread, spread) * hipMultiplier;

				// Raycast
				Ray ray = new Ray (player.cam.transform.position, Quaternion.Euler (tracerRot) * Vector3.forward);
				RaycastHit hit;
				Vector3 hitPoint = Vector3.zero;

				if (Physics.Raycast (ray, out hit, 500f, weaponController.hitMask, QueryTriggerInteraction.Collide)) {
					// Take damage

					LivingEntity entity = hit.collider.GetComponent<LivingEntity>();
					hitPoint = hit.point;
					if (entity != null) {
						CmdShootPrimary (hit.collider.name);
					}
				}
				// Tracer spawning
				Vector3 tracerSpawn = player.cam.transform.position;
				tracerSpawn.y -= .15f;

				if (hitPoint == Vector3.zero) {
					hitPoint = player.cam.transform.forward * 500;

				}

				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}


				// Tell server to spawn tracer for all clients
				CmdOnProjectileHit (hitPoint, owner.name, Quaternion.Euler (tracerRot), impactRot);
			}
				
			// Audio
			if (shootSound != null) {
				AudioManager.instance.PlaySound2D (shootSound, 1);
			}

			// Animation
			lastShotTime = Time.time;
		}
	}

	[Command]
	void CmdShootPrimary(string id) {
		if (GameManager.GetCharacter (id) != null) {
			//GameManager.GetCharacter (id).TakeDamage (damage, transform.name);
		}
	}

	[Command]
	void CmdOnProjectileHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		RpcOnProjectileHit (hitPoint, id, tracerRot, impactRot);

	}

	[ClientRpc]
	void RpcOnProjectileHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {

		Vector3 pos = GameManager.GetCharacter (id).GetComponent<Player>().cam.transform.position;
		pos.y -= .15f;

		// Spawn tracer
		if (tracer != null) {
			Tracer tracerClone = Instantiate (tracer, pos, tracerRot) as Tracer;
			tracerClone.SetupTracer (hitPoint, tracerSpeed);
		}

		// Spawn impactFX
		if (impactFX != null) {
			Instantiate (impactFX, hitPoint, impactRot);
		}
	}
}
