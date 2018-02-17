using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : LivingEntity {
	[Header ("Main Properties")]
	public Camera cam;
	[HideInInspector]
	public short myMsgId = 1000;

	public override void OnStartClient() {
		string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
		LivingEntity entity = GetComponent<LivingEntity> ();
		GameManager.RegisterPlayer (myID, entity, entityName);
	}

	public override void Start() {
		base.Start ();
		if (gameObject.activeSelf) {
			if (isLocalPlayer) {
				NetworkManager.singleton.client.RegisterHandler (myMsgId, GetComponent<GunController> ().OnSpawnNewWeapon);
				GameManager.instance.SetLocalPlayer(this);
				CmdSetAuthority (GameManager.instance.netId, GetComponent<NetworkIdentity> ());
			}
		}
	}

	public void SetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		CmdSetAuthority (objectId, targetPlayer);
	}

	[Command]
	void CmdSetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		GameObject targetObject = NetworkServer.FindLocalObject (objectId);
		if (targetObject != null) {
			NetworkIdentity targetIdentity = targetObject.GetComponent<NetworkIdentity> ();
			var currentOwner = targetIdentity.clientAuthorityOwner;

			if (currentOwner == targetPlayer.connectionToClient) {
				return;
			} else {
				if (currentOwner != null) {
					targetIdentity.RemoveClientAuthority (currentOwner);
				}
				targetIdentity.AssignClientAuthority (targetPlayer.connectionToClient);
			}
		}
	}


	[Command]
	public void CmdKillPlayer() {
		// Penalties
		GameManager.instance.TakeMoney (75);

		// Reset player stats
		RpcKillPlayer ();
		PlayerStats ps = GetComponent<PlayerStats> ();
		ps.FatiqueAdd (100);
		ps.StaminaAdd (100);
		ps.HungerAdd (100);
	}

	[ClientRpc]
	void RpcKillPlayer() {
		// Penalties
		PlayerInventory playerInventory = GetComponent<PlayerInventory> ();
		playerInventory.FlushInventory ();

		transform.position = new Vector3 (0, .6f, 0);
	}
}
