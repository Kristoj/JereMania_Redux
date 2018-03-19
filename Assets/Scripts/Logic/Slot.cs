using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

	public Item slotItem;
	public Image slotIcon;
	public int slotIndex;
	private Container container;

	void Start() {
		slotIcon = transform.GetChild (0).GetComponent<Image> ();
	}

	public void SetSlotIcon(Sprite newIcon) {
		
		if (slotIcon != null) {
			slotIcon.sprite = newIcon;
			slotIcon.gameObject.SetActive (true);
			if (newIcon != null) {
				Debug.Log ("SEt new icon: " + newIcon.name);
			}
		} else {
			Debug.Log ("SLOT ICON IS NULL");
		}
	}

	public void SetupSlot(Container inv, int index) {
		container = inv;
		slotIndex = index;
	}

	public void AddItem(Item itemToAdd) {
		slotItem = itemToAdd;
		SetSlotIcon (itemToAdd.itemIcon);
	}

	public void ClearSlot () {
		slotItem = null;
		slotIcon.gameObject.SetActive (false);
	}

	public void ExhangeSlot(Slot startSlot) {

		// Copy startSlot
		Item startSlotItem = startSlot.slotItem;
		Image startSlotIcon = startSlot.slotIcon;

		// Override startSlot
		startSlot.slotItem = this.slotItem;
		startSlot.slotIcon = this.slotIcon;

		// Override this slot
		this.slotItem = startSlotItem;
		this.slotIcon = startSlotIcon;

		startSlot.slotIcon.transform.SetParent (startSlot.transform);
		startSlot.slotIcon.transform.localPosition = Vector3.zero;
		this.slotIcon.transform.SetParent (this.transform);
		this.slotIcon.transform.localPosition = Vector3.zero;
	}

	IEnumerator DragDelay() {

		float t = .1f;
		while (t > 0) {
			t -= Time.deltaTime;
			if (Input.GetButtonUp ("Fire1")) {
				yield break;
			}
			yield return null;
		}

		if (slotItem != null) {
			container.StartCoroutine ("DragItem", this);
			print ("Slot Start");
		}
	}

	public void OnPointerDown (PointerEventData eventData) {
		// Left click
		if (eventData.pointerId == -1) {
			StartCoroutine (DragDelay ());
		}

		if (eventData.pointerId == -2) {
			container.DropItem (this.slotIndex);
		}
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (container.playerInventory != null) {
			container.playerInventory.targetSlot = this;
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (container.playerInventory != null) {
			container.playerInventory.targetSlot = null;
		}
	}
}