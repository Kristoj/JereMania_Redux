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
			PlayerInventory targetInventory = GameManager.GetPlayerByName (masterId).GetComponent<PlayerInventory>();
			if (!targetInventory.isFull) {
				PickUpItem (targetInventory);
			}
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

	/// <summary>
	/// Drops the item for all clients.
	/// </summary>
	/// <param name="objName">Entity name of the object to drop.</param>
	/// <param name="masterId">Players name who called this function.</param>
	/// <param name="dropPos">Position where the item will be dropped.</param>
	/// <param name="dropRot">Rotation of the item when dropped.</param>
	/// <param name="dropDir">Direction where force is applied.</param>
	/// <param name="dropForce">Amount of force added when dropped.</param>
	[Command]
	public void CmdDropItem(string entityName, string masterId, Vector3 dropPos, Quaternion dropRot, Vector3 dropDir, float dropForce) {
		// Spawn item on client and then on the server
		Item clone = Instantiate (EquipmentLibrary.instance.GetEquipment (entityName).GetComponent<Item> (), dropPos, dropRot) as Item;
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
			rig.AddForce (dropDir / massMultiplier * rig.mass * dropForce + (GameManager.GetPlayerByName(masterId).GetComponent<CharacterController>().velocity * rig.mass), ForceMode.Impulse);
		}
	}

	[Command]
	public void CmdOnPickup() {
		DestroyEntity ();
	}
}
