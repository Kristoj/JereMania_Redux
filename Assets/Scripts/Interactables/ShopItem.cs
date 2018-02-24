using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShopItem : Interactable {

	public float price = 25f;
	public Rigidbody saleItem;

	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);
		BuyItem ();
	}

	void BuyItem() {
		if (saleItem != null) {
			CmdSpawnItem ();
		}
	}

	[Command]
	void CmdSpawnItem() {
		if (GameManager.instance.money >= price) {
			Rigidbody clone = Instantiate (saleItem, transform.position + transform.up, transform.rotation);
			NetworkServer.Spawn (clone.gameObject);
			RpcSpawnItem (clone.GetComponent<NetworkIdentity> ().netId);
			GameManager.instance.TakeMoney (price);
		}
	}

	[ClientRpc]
	void RpcSpawnItem(NetworkInstanceId cloneId) {
		Rigidbody clone = ClientScene.FindLocalObject (cloneId).GetComponent<Rigidbody>();
		clone.AddForce (Vector3.up * clone.mass * 6, ForceMode.Impulse);
		clone.AddForce (-transform.forward * clone.mass * 1, ForceMode.Impulse);
		clone.AddTorque (Random.insideUnitSphere * 15 * clone.mass, ForceMode.Impulse);
	}
}