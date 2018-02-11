using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeleportTrigger : MonoBehaviour {

	public Vector3 teleportPos;

	void OnTriggerEnter (Collider c) {
		if (c.tag == "Player") {
			string id = c.name;
			CmdTeleportPlayer (id);
		}
	}

	void CmdTeleportPlayer(string id) {
		RpcTeleportPlayer (id);
	}

	void RpcTeleportPlayer(string id) {
		GameManager.GetEntity (id).transform.position = teleportPos;
	}
}
