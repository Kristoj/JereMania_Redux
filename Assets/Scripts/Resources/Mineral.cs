using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mineral : LivingEntity {

	public Transform deathParticle;

	public override void Die() {
		if (deathParticle != null) {
			Instantiate (deathParticle, transform.position, transform.rotation);
		}
		base.Die ();
	}
}
