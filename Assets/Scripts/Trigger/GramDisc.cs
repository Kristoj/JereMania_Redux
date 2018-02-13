using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GramDisc : NetworkBehaviour {

	void OnTriggerEnter (Collider c) {
		if (isServer) {
			AudioSource aud = c.GetComponent<AudioSource> ();
			Equipment Disc = c.GetComponent<Equipment> ();
			Rigidbody rg = c.GetComponent<Rigidbody>();
			Item discItem = c.GetComponent<Item>();
			aud.enabled = false;
			if (discItem != null) {
				if(Disc.isAvailable == true){
					if (discItem.objectName == "Disc_Jere" || discItem.objectName == "Disc_Elmu") {
						rg.isKinematic = true;
						Vector3 discPosition = transform.position + new Vector3 (0, 0.5949595f, 0);
						c.transform.position = discPosition;
						c.transform.eulerAngles = transform.eulerAngles;
						aud.enabled = true;
						RpcInsertDisk (c.name, discPosition, transform.rotation);
					}
				}
			}
		}
	}

	[ClientRpc]
	void RpcInsertDisk(string diskName, Vector3 discPosition, Quaternion diskRot) {
		GameObject diskObject = GameManager.GetEntity (diskName).gameObject;
		if (diskObject != null) {
			Rigidbody rg = diskObject.GetComponent<Rigidbody> ();
			rg.isKinematic = true;
			rg.transform.position = discPosition;
			rg.transform.rotation = diskRot;
		}

		// Play sound
		AudioManager.instance.CmdPlaySound2D ("Recordscratch", transform.position, GameManager.instance.localPlayer.name, 1);
	}
}
