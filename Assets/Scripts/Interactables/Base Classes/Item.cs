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
		base.Start ();
		rig = GetComponent<Rigidbody>();
	}

	public override void OnServerStartInteraction(string masterId) {
		if (canStoreInInventory && isAvailable) {
			PlayerInventory targetInventory = GameManager.GetPlayerByName(masterId).GetComponent<PlayerInventory>();
			if (!targetInventory.isFull) {
				base.OnServerStartInteraction (masterId);
				PickUpItem (targetInventory);
			}
		}
	}

	void PickUpItem(PlayerInventory inv) {
		if (isAvailable) {
			for (int i = 0; i < inv.slots.Count; i++) {
				if (inv.slots [i].slotItem == null) {
					inv.AddItem (this.name, i, this.entityGroupIndex);
					CmdOnPickup ();
					break;
				}
			}
		}
	}

	/// <summary>
	/// Drops the item for all clients. Must be called from server
	/// </summary>
	/// <param name="objName">Entity name of the object to drop.</param>
	/// <param name="masterId">Players name who called this function.</param>
	/// <param name="dropPos">Position where the item will be dropped.</param>
	/// <param name="dropRot">Rotation of the item when dropped.</param>
	/// <param name="dropDir">Direction where force is applied.</param>
	/// <param name="dropForce">Amount of force added when dropped.</param>
	public void DropItem(string masterId, Vector3 dropPos, Quaternion dropRot, Vector3 dropDir, float dropForce) {
		// Add force to the spawned item
		EnableEntity ();
		RpcDropItem (masterId, dropPos, dropRot, dropDir, dropForce);
	}

	[ClientRpc]
	public void RpcDropItem(string masterId, Vector3 dropPos, Quaternion dropRot, Vector3 dropDir, float dropForce) {
		rig = GetComponent<Rigidbody> ();
		if (rig != null) {
			rig.isKinematic = false;
			transform.parent = null;
			isAvailable = true;

			// Set layers
			gameObject.layer = LayerMask.NameToLayer ("Default");
			if (transform.childCount > 0) {
				for (int i = 0; i < transform.childCount; i++) {
					transform.GetChild (i).gameObject.layer = LayerMask.NameToLayer ("Default");
				}
			}

			// Position
			transform.position = dropPos;

			// Add force
			float massMultiplier = rig.mass;
			massMultiplier = Mathf.Clamp (massMultiplier, .3f, 1.4f) * .8f;
			Player targetPlayer = GameManager.GetPlayerByName (masterId);
			if (targetPlayer != null) {
				rig.AddForce (dropDir / massMultiplier * rig.mass * dropForce + (targetPlayer.GetComponent<CharacterController>().velocity * rig.mass), ForceMode.Impulse);

				//rig.maxAngularVelocity = 100;
				rig.AddTorque (transform.right * 250, ForceMode.Impulse);
			}

		}
	}

	[Command]
	public void CmdOnPickup() {
		DisableEntity ();
	}
}
