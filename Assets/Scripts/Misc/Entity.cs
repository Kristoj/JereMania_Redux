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
	[HideInInspector]
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


	public virtual void OnEntityHit(string playerName) {

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
	/// Sets the authority for the entity. Must be called from server
	/// </summary>
	// Set authority for the player who wants to control this entity
	public void SetAuthorityFromServer(string playerName) {
		Player targetPlayer = GameManager.GetLocalPlayer ();
		if (targetPlayer != null) {
			targetPlayer.SetAuthority (netId, targetPlayer.GetComponent<NetworkIdentity>());
		}
	}

	public void SetAuthorityFromClient() {
		string targetPlayer = GameManager.GetLocalPlayer ().name;
		CmdSetAuthority (targetPlayer);
	}

	[Command]
	private void CmdSetAuthority(string playerName) {
		Player targetPlayer = GameManager.GetPlayerByName (playerName);
		if (targetPlayer != null) {
			targetPlayer.SetAuthority (netId, targetPlayer.GetComponent<NetworkIdentity>());
		}
	}

	public void SetEntityParent(Transform t) {
		transform.parent = t;
	}
}
