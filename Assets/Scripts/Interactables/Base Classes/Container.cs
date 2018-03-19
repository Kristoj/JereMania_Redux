using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Container : Interactable {

	[Header ("Inventory Properties")]
	public int slotCountX = 5;
	public int slotCountY = 5;
	public float slotSize = 30f;
	public List<Slot> slots = new List<Slot>();
	public float slotGap = .25f;
	protected bool isOpen = false;
	public bool isFull = false;

	[Header ("Assignables")]
	public RectTransform containerWindow;
	public RectTransform slotHolder;
	public RectTransform slot;

	// Components
	public PlayerInventory playerInventory;
	protected bool isPlayer;

	// Use this for initialization
	public override void Start () {
		if (GetComponent<PlayerInventory> () != null) {
			isPlayer = true;
		}
		if (containerWindow != null) {
			containerWindow.gameObject.SetActive (true);
			SetupInventory ();
			ShowInventory (false);
		}
	}

	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);
		playerInventory = owner.GetComponent<PlayerInventory> ();
		ShowInventory (true);

	}

	public override void OnExit(string masterId) {
		base.OnExit (masterId);
		//ShowInventory (false);
	}

	public virtual void ShowInventory(bool ab) {
		// OPEN Inventoy
		if (ab) {
			containerWindow.gameObject.SetActive (true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			isOpen = true;

			if (playerInventory != null) {
				playerInventory.ShowInventory (true);
				playerInventory.OnContainerOpen (this);
			}
		} 
		// CLOSE Inventoy
		else {
			// Misc
			containerWindow.gameObject.SetActive (false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			isOpen = false;
			if (playerInventory != null) {
				//playerInventory.ShowInventory (false);
			}
		}
	}

	void SetupInventory() {
		// Get slot starting position
		Vector3 startPos;
		startPos.x = -((slotHolder.sizeDelta.x / 2) - slotSize);
		startPos.y = ((slotHolder.sizeDelta.y / 2) - slotSize);
		startPos.z = 0;

		// Instantiate slots
		int iCount = 0;
		for (int i = 0; i < slotCountY; i++) {
			for (int j = 0; j < slotCountX; j++) {
				RectTransform slotClone = Instantiate (slot, slotHolder.position, slotHolder.rotation, slotHolder.transform) as RectTransform;
				slotClone.sizeDelta = new Vector2 (slotSize, slotSize);
				Vector3 targetSlotPos = new Vector3 (startPos.x + (slotClone.sizeDelta.x * j + slotGap), startPos.y + ((slotClone.sizeDelta.y * -i - slotGap)), 0);
				slotClone.transform.localPosition = targetSlotPos;

				Slot cloneSlot = slotClone.GetComponent<Slot> ();
				cloneSlot.SetupSlot (this, iCount);
				cloneSlot.slotIcon.rectTransform.sizeDelta = new Vector2 (slotSize, slotSize) * .75f;
				slots.Add (slotClone.GetComponent<Slot> ());
				iCount++;
			}
		}
	}

	public void AddItem (string itemName, int slotIndex, int itemGroupIndex) {
		Entity itemToAdd = GameManager.instance.GetEntity (itemName, itemGroupIndex) as Entity;
		if (itemToAdd != null) {
			slots [slotIndex].AddItem (itemToAdd.GetComponent<Item>());
			RpcAddItem (itemToAdd.entityName, slotIndex);
			RefreshOpenSlots ();
		}
	}

	[ClientRpc]
	void RpcAddItem(string itemName, int slotIndex) {
		if (!isServer) {
			slots [slotIndex].SetSlotIcon (EquipmentLibrary.instance.GetEquipment (itemName).itemIcon);
		}
	}

	public void DropItem(int slotIndex) {
		CmdDropItem (slotIndex);
	}

	void RefreshOpenSlots() {
		int openSlots = 0;
		// Check if our inventory is full or not
		foreach (Slot s in slots) {
			if (s.slotItem == null) {
				openSlots++;
			}
		}
		if (openSlots <= 0) {
			isFull = true;
		} else {
			isFull = false;
		}
	}

	[Command]
	public void CmdDropItem(int slotIndex) {
		Slot dropSlot = slots [slotIndex];
		if (dropSlot.slotItem != null) {
			Camera playerCam = GetComponent<Player> ().cam;
			if (playerCam != null) {
				dropSlot.slotItem.DropItem (owner.name, playerCam.transform.position + playerCam.transform.forward, Quaternion.identity, playerCam.transform.forward, 1);
				RefreshOpenSlots ();
			}
			RpcDropItem (dropSlot.slotItem.entityName, slotIndex);
		}
	}

	[ClientRpc]
	void RpcDropItem(string itemName, int slotIndex) {
		slots [slotIndex].ClearSlot ();
	}

	/**
	void MoveItem(Slot startSlot, Slot endSlot) {
		if (endSlot.slotItem == null) {
			endSlot.AddItem (startSlot.slotItem.entityName);
			startSlot.ClearSlot ();
		}
	}
	**/

	public void FlushInventory() {
		foreach (Slot s in slots) {
			s.ClearSlot ();
			isFull = false;
		}
	}

	/**
	public IEnumerator DragItem(Slot startSlot) {

		// Drag item while holding left mouse down
		Image dragIcon = startSlot.slotIcon;
		dragIcon.transform.SetParent (playerInventory.containerWindow.transform);
		while (true) {
			if (Input.GetButtonUp ("Fire1")) {
				if (playerInventory.targetSlot != null) {
					print ("Not Null");
					// Move back to orininal slot
					if (startSlot == playerInventory.targetSlot) {
						dragIcon.transform.SetParent (startSlot.transform);
						dragIcon.transform.localPosition = Vector3.zero;
						yield return null;
					}

					// Move item
					else if (playerInventory.targetSlot.slotItem == null) {
						print ("Move");
						//MoveItem (startSlot, playerInventory.targetSlot);
						dragIcon.transform.SetParent (startSlot.transform);
						dragIcon.transform.localPosition = Vector3.zero;
					} 
					// Exhange slot items 
					else {
						playerInventory.targetSlot.ExhangeSlot (startSlot);
						print ("Exhange");
					}
				} else {
					dragIcon.transform.SetParent (startSlot.transform);
					dragIcon.transform.localPosition = Vector3.zero;
				}
				yield break;
			}
			dragIcon.transform.position = Input.mousePosition + new Vector3 (6, -6, 0);
			yield return null;
		}
	}
	**/
}
