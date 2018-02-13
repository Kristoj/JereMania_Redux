using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Item : Interactable {

	[Header ("Item Properties")]
	public float price = 10f;
	public bool canStoreInInventory = true;
	[HideInInspector]
	public int itemId = 0;

	[Header ("UI")]
	public Sprite itemIcon;

	public override void Start() {
		rig = GetComponent<Rigidbody>();
	}

	public override void OnStartInteraction(string masterId) {
		if (canStoreInInventory && isAvailable) {
			PickUpItem (GameManager.GetCharacter (masterId).GetComponent<PlayerInventory> ());
		}
	}

	public override void OnStartPickup(string masterId) {
		if (isAvailable) {
			base.OnStartPickup (masterId);
		}
	}

	void PickUpItem(PlayerInventory inv) {
		if (isAvailable) {
			for (int i = 0; i < inv.slots.Count; i++) {
				if (inv.slots [i].slotItem == null) {
					inv.AddItem (this, i);
					CmdOnPickup ();
					break;
				}
			}
		}
	}

	[Command]
	public void CmdDropItem(string objName, string masterId, Vector3 dropPos, Quaternion dropRot, Vector3 dropDir, float dropForce) {
		// Spawn item on client and then on the server
		Item clone = Instantiate (EquipmentLibrary.instance.GetEquipment (objName).GetComponent<Item> (), dropPos, dropRot) as Item;
		NetworkServer.Spawn (clone.gameObject);
		// Add force to the spawned item
		clone.RpcDropItem (masterId, dropPos, dropRot, dropDir, dropForce);
	}

	[ClientRpc]
	public void RpcDropItem(string masterId, Vector3 dropPos, Quaternion dropRot, Vector3 dropDir, float dropForce) {
		rig = GetComponent<Rigidbody> ();
		if (rig != null) {
			rig.isKinematic = false;
			float massMultiplier = rig.mass;
			massMultiplier = Mathf.Clamp (massMultiplier, .3f, 1.4f);
			rig.AddForce (dropDir / massMultiplier * rig.mass * dropForce + (GameManager.GetPlayer(masterId).GetComponent<CharacterController>().velocity * rig.mass), ForceMode.Impulse);
		}
	}

	[Command]
	public void CmdOnPickup() {
		OnEntityDestroy ();
		NetworkServer.Destroy (this.gameObject);
	}
}
