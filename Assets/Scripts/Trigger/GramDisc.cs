using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GramDisc : NetworkBehaviour {

	void OnTriggerEnter (Collider c) {
		if (isServer) {
			Equipment Disc = c.GetComponent<Equipment> ();
			Rigidbody rg = c.GetComponent<Rigidbody>();
			Item discItem = c.GetComponent<Item>();
			if (discItem != null) {
				if(Disc.isAvailable == true){
					if (discItem.entityName == "Disc_Jere" || discItem.entityName == "Disc_Elmu") {
						rg.isKinematic = true;
						Vector3 discPosition = transform.position + new Vector3 (0, 0.5949595f, 0);
						c.transform.position = discPosition;
						c.transform.eulerAngles = transform.eulerAngles;
						RpcInsertDisk (c.name, discItem.entityGroupIndex, discPosition, transform.rotation);
					}
				}
			}
		}
	}

	[ClientRpc]
	void RpcInsertDisk(string diskName, int diskGroupIndex, Vector3 discPosition, Quaternion diskRot) {
		GameObject diskObject = GameManager.instance.GetEntity (diskName, diskGroupIndex).gameObject;
		if (diskObject != null) {
			Rigidbody rg = diskObject.GetComponent<Rigidbody> ();
			rg.isKinematic = true;
			rg.transform.position = discPosition;
			rg.transform.rotation = diskRot;
			AudioSource aud = diskObject.GetComponent<AudioSource> ();
			aud.enabled = true;
		}

		// Play sound
		AudioManager.instance.CmdPlaySound2D ("Recordscratch", transform.position, GameManager.GetLocalPlayer().name, 1);
	}
}
