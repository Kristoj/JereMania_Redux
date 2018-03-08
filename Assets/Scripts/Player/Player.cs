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
		Player entity = GetComponent<Player> ();
		GameManager.RegisterPlayer (myID, entity, entityName);
	}

	public override void Start() {
		base.Start ();
		if (gameObject.activeSelf) {
			if (isLocalPlayer) {
				NetworkManager.singleton.client.RegisterHandler (myMsgId, OnMessageReceive);
				GameManager.instance.SetLocalPlayer(this);
				SetAuthority (GameManager.instance.netId, GetComponent<NetworkIdentity> ());
			}
		}
	}

	public void SetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		if (isServer) {
			AcceptAuthority (objectId, targetPlayer);
		} else {
			CmdSetAuthority (objectId, targetPlayer);
		}
	}

	[Command]
	void CmdSetAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
		AcceptAuthority (objectId, targetPlayer);
	}

	void AcceptAuthority(NetworkInstanceId objectId, NetworkIdentity targetPlayer) {
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

	/// <summary>
	/// Is called when local client receives a message from the server.
	/// </summary>
	/// <param name="netMsg">Message that is send to the client.</param>
	public void OnMessageReceive(NetworkMessage netMsg) {
		
	}
		
	public void KillPlayer() {
		// Penalties
		GameManager.instance.TakeMoney (75);
		PlayerInventory playerInventory = GetComponent<PlayerInventory> ();
		playerInventory.FlushInventory ();

		// Reset player stats
		PlayerStats ps = GetComponent<PlayerStats> ();
		ps.FatiqueAdd (100);
		ps.StaminaAdd (100);
		ps.HungerAdd (100);

		// Reset player stats
		CmdKillPlayer ();
	}

	[Command]
	void CmdKillPlayer() {
		RpcKillPlayer ();
	}

	[ClientRpc]
	void RpcKillPlayer() {
		transform.position = new Vector3 (0, .6f, 0);
	}
}
