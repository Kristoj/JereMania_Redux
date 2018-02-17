using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SellingPoint : NetworkBehaviour {

	void OnTriggerEnter (Collider c) {
		if (isServer) {
			Item sellItem = c.GetComponent<Item> ();
			if (sellItem != null) {
				if (sellItem.entityName == "Iron_Ore" || sellItem.entityName == "Potato" ||sellItem.entityName == "Carrot" && sellItem.isAvailable) {
					AudioManager.instance.CmdPlaySound2D ("UI_Transaction", transform.position, GameManager.GetLocalPlayer().name, 1);
					GameManager.instance.GiveMoney (sellItem.price);
					NetworkServer.Destroy (sellItem.gameObject);
				}
			}
		}
	}
}
