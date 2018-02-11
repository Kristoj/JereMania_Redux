using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entity : NetworkBehaviour {

	public string prefix = "default";
	public delegate void DeathDelegate ();
	public DeathDelegate deathEvent;

	protected Rigidbody rig;
	public AudioClip hurtSound;

	public virtual void Start() {
		rig = GetComponent<Rigidbody> ();

	}

	public override void OnStartClient() {
		base.OnStartClient ();

		if (GetComponent<LivingEntity> () == null) {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			Entity entity = GetComponent<Entity> ();
			GameManager.RegisterEntity (myID, entity, prefix);
		} else {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			LivingEntity entity = GetComponent<LivingEntity> ();
			GameManager.RegisterCharacter (myID, entity, prefix);
		}
	}

	public void OnEntityDestroy() {
		if (deathEvent != null) {
			deathEvent ();
		}
	}

	public void AddImpactForce(Vector3 impactForce, Vector3 impactPos) {
		RpcAddImpactForce (impactForce, impactPos);
	}

	[ClientRpc]
	void RpcAddImpactForce(Vector3 impactForce, Vector3 impactPos) {
		// Add force
		if (rig == null) {
			rig = GetComponent<Rigidbody> ();
		}

		if (rig != null) {
			rig.AddForceAtPosition (impactForce, impactPos, ForceMode.Impulse);
		}
	}
}
