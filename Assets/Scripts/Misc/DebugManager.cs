using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DebugManager : NetworkBehaviour {

	private NetworkManager netManager;

	void Start() {
		netManager = GameObject.Find ("#NETWORKMANAGER").GetComponent<NetworkManager> ();
	}

	// Update is called once per frame
	void Update () {
		if (isServer) {
			if (Input.GetKeyDown (KeyCode.Keypad0)) {
				netManager.ServerChangeScene ("Krifi_Scene");
			}

			if (Input.GetKeyDown (KeyCode.Keypad1)) {
				netManager.ServerChangeScene ("Vili_Scene");
			}
		}
	}
}
