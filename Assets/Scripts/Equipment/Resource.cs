using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MeleeWeapon {

	public override IEnumerator StartAttack() {
		if (fireMode == FireMode.Semi && !weaponController.mouseLeftReleased) {
			yield break;
		}

		if (Time.time - lastShotTime > timeBetweenShots) {

			playerAnimationController.Attack();
			lastShotTime = Time.time;
			// Audio
			if (attackSound != null) {
				AudioManager.instance.PlaySound2D (attackSound, 1);
			}

			yield return new WaitForSeconds (hitDelay);
			// Rot
			Vector3 tracerRot = new Vector3 (player.cam.transform.eulerAngles.x, player.cam.transform.eulerAngles.y, player.cam.transform.eulerAngles.z);

			// Raycast
			Ray ray = new Ray (player.cam.transform.position, Quaternion.Euler (tracerRot) * Vector3.forward);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, meleeRange, weaponController.hitMask, QueryTriggerInteraction.Collide)) {
				// Take damage
				playerAnimationController.MeleeImpact();

				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}

				// Audio
				if (attackSound != null) {
					AudioManager.instance.PlaySound2D (impactAudio, 1);
				}

				// Tell server to spawn tracer for all clients
				CmdOnProjectileHit (hit.point, transform.name, Quaternion.Euler (tracerRot), impactRot);
			}
		}
	}
}
