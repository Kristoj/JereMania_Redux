using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerInventory : Container {

	private Player player;
	private Container targetContainer;
	public Slot targetSlot;

	public override void OnStartClient() {

	}

	public override void OnStartServer() {
		base.OnStartServer ();
	}

	public override void Start() {
		player = GetComponent<Player> ();
		playerInventory = GetComponent<PlayerInventory> ();
		owner = transform;

		base.Start ();
		ShowInventory (false);
	}

	public override void ShowInventory(bool ab) {
		if (ab) {
			containerWindow.gameObject.SetActive (true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			isOpen = true;
			player.isActive = false;
		} else {
			containerWindow.gameObject.SetActive (false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			isOpen = false;
			player.isActive = true;
			if (targetContainer != null) {
				targetContainer.ShowInventory (false);
			}
		}
	}

	void Update() {
		if (!isLocalPlayer) {
			return;
		}
		if (Input.GetKeyDown (KeyCode.Tab)) {
			ShowInventory (!isOpen);
		}
	}

	public void OnContainerOpen(Container c) {
		targetContainer = c;
	}
}
