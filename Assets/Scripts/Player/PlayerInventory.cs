using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerInventory : Container {

	private PlayerController playerController;
	private Container targetContainer;
	public Slot targetSlot;

	public override void OnStartClient() {
		
	}

	public override void Start() {
		playerController = GetComponent<PlayerController> ();
		playerInventory = GetComponent<PlayerInventory> ();
		base.Start ();
		ShowInventory (false);
	}

	public override void ShowInventory(bool ab) {
		if (ab) {
			containerWindow.gameObject.SetActive (true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			isOpen = true;
			playerController.SetPlayerEnabled (false);
		} else {
			containerWindow.gameObject.SetActive (false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			isOpen = false;
			playerController.SetPlayerEnabled (true);
			if (targetContainer != null) {
				targetContainer.ShowInventory (false);
			}
		}
	}
	public override void Update() {
		if (Input.GetKeyDown (KeyCode.Tab)) {
			ShowInventory (!isOpen);
		}
	}

	public void OnContainerOpen(Container c) {
		targetContainer = c;
	}
}
