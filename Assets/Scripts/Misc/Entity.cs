﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entity : NetworkBehaviour {

	public string entityName = "Entity";
	public delegate void DeathDelegate ();
	public DeathDelegate deathEvent;
	public EntitySoundMaterial entitySoundMaterial;
	public enum EntitySoundMaterial {Wood, Metal, Rock}

	protected Rigidbody rig;
	public AudioClip hurtSound;
	//[HideInInspector]
	[SyncVar]
	public int entityGroupIndex;

	public virtual void Start() {
		rig = GetComponent<Rigidbody> ();
	}

	public override void OnStartClient() {
		base.OnStartClient ();

		if (GetComponent<LivingEntity> () == null) {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			Entity entity = GetComponent<Entity> ();
			GameManager.instance.RegisterEntity (myID, entity, entityName);

		} else {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			LivingEntity entity = GetComponent<LivingEntity> ();
			GameManager.instance.RegisterLivingEntity (myID, entity, entityName);
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
		
	public void SetGroupId (int i) {
		entityGroupIndex = i;
	}

	/// <summary>
	/// Destroys the entity from all clients and unregisters it from the gamemanager.
	/// Must be called from the server!
	/// </summary>
	public virtual void DestroyEntity() {
		OnEntityDestroy ();
		GameManager.instance.RemoveEntity (this, entityGroupIndex);
		NetworkServer.Destroy (this.gameObject);
	}

	/// <summary>
	/// Destroys the entity from all clients and unregisters it from the gamemanager.
	/// /// Must be called from a client!
	/// </summary>
	/// <param name="callerNetId">NetId of the player who called this function.</param>
}
