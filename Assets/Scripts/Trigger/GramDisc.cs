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
						AudioManager.instance.CmdPlaySound2D ("UI_Transaction", transform.position, GameManager.instance.localPlayer.name, 1);
						c.transform.position = transform.position + new Vector3 (0, 0.5949595f, 0);
						c.transform.eulerAngles = transform.eulerAngles;
						aud.enabled = true;

					}
				}
			}
		}
	}
}
