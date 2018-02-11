using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof(NetworkIdentity))]
public class Timber : LivingEntity {

	public int dropCount = 5;
	public Vector3 dropOffset;
	public Rigidbody dropResource;

	public override void Die() {
		base.Die ();

		// Drop resources
		for (int i = 0; i < dropCount; i++) {
			CmdDropResource ();
		}
	}

	[Command]
	void CmdDropResource() {
		if (dropResource != null) {
			Rigidbody clone = Instantiate (dropResource, transform.position + dropOffset, Quaternion.identity) as Rigidbody;
			clone.AddForce (Random.insideUnitSphere * Random.Range (-4, 4), ForceMode.Impulse);
			clone.AddRelativeForce (Random.insideUnitSphere * Random.Range (-4, 4), ForceMode.Impulse);
			NetworkServer.Spawn (clone.gameObject);
		}
	}

}
