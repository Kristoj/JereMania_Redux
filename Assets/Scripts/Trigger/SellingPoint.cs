﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SellingPoint : NetworkBehaviour {

	void OnTriggerEnter (Collider c) {
		if (isServer) {
			Item sellItem = c.GetComponent<Item> ();
			if (sellItem != null) {
				if (sellItem.objectName == "Iron_Ore" || sellItem.objectName == "Potato" ||sellItem.objectName == "Carrot" && sellItem.isAvailable) {
					AudioManager.instance.CmdPlayCustomSound2D ("UI_Transaction", transform.position, GameManager.instance.localPlayer.name, 1);
					GameManager.instance.GiveMoney (sellItem.price);
					NetworkServer.Destroy (sellItem.gameObject);
				}
			}
		}
	}
}
