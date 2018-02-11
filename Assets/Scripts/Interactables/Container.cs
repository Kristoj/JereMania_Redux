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
			SetupInventory ();
			ShowInventory (false);
		}
	}

	public virtual void Update() {
		if (!isPlayer && playerInventory != null) {
			//Vector3 difference = new Vector3 (playerInventory.transform.position.x, containerWindow.transform.position.y,  playerInventory.transform.position.z) - new Vector3 (containerWindow.transform.position.x, 0,  containerWindow.transform.position.z);
			//Quaternion lookRot = Quaternion.LookRotation (difference);
			//containerWindow.transform.rotation = lookRot;
		}
	}

	public override void OnStartInteraction(string masterId) {
		base.OnStartInteraction (masterId);
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
		for (int i = 0; i < slotCountY; i++) {
			for (int j = 0; j < slotCountX; j++) {
				RectTransform slotClone = Instantiate (slot, slotHolder.position, slotHolder.rotation, slotHolder.transform) as RectTransform;
				slotClone.sizeDelta = new Vector2 (slotSize, slotSize);
				Vector3 targetSlotPos = new Vector3 (startPos.x + (slotClone.sizeDelta.x * j + slotGap), startPos.y + ((slotClone.sizeDelta.y * -i - slotGap)), 0);
				slotClone.transform.localPosition = targetSlotPos;

				Slot cloneSlot = slotClone.GetComponent<Slot> ();
				cloneSlot.SetupSlot (this);
				cloneSlot.slotIcon.rectTransform.sizeDelta = new Vector2 (slotSize, slotSize) * .75f;
				slots.Add (slotClone.GetComponent<Slot> ());
			}
		}
	}

	public void AddItem (Item itemToAdd, int index) {
		slots [index].AddItem (itemToAdd.objectName);
	}

	public void DropItem(Slot dropSlot) {
		if (dropSlot.slotItem != null) {
			Camera playerCam = GetComponent<Player> ().cam;
			CmdDropItem (playerCam.transform.position + playerCam.transform.forward, dropSlot.slotItem.objectName);
			dropSlot.ClearSlot ();
		}
	}

	[Command]
	public void CmdDropItem(Vector3 dropPos, string objName) {
		Rigidbody clone = Instantiate (EquipmentLibrary.instance.GetEquipment (objName).GetComponent<Rigidbody> (), dropPos, Quaternion.identity) as Rigidbody;
		clone.isKinematic = false;
		//clone.AddForce (
		NetworkServer.Spawn (clone.gameObject);
	}

	void MoveItem(Slot startSlot, Slot endSlot) {
		if (endSlot.slotItem == null) {
			endSlot.AddItem (startSlot.slotItem.objectName);
			startSlot.ClearSlot ();
		}
	}

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
						MoveItem (startSlot, playerInventory.targetSlot);
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
}
