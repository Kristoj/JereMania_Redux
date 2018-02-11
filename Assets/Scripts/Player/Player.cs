using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : LivingEntity {
	[Header ("Main Properties")]
	public Camera cam;
	[HideInInspector]
	public short myMsgId = 1000;

	public override void Start() {
		base.Start ();
		if (gameObject.activeSelf) {
			if (isLocalPlayer) {
				NetworkManager.singleton.client.RegisterHandler (myMsgId, GetComponent<GunController> ().OnSpawnNewWeapon);
				GameManager.instance.localPlayer = this.transform;
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
		NetworkIdentity targetIdentity = targetObject.GetComponent<NetworkIdentity>();
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
