using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChildSeat : ChildInteractable {

	public Vector3 sitPosition;
	public Vector3 exitPosition;
	public float viewHeight = .6f;

	public override void OnServerStartInteraction(string sourcePlayer) {
		// Make sure to call base functions
		base.OnServerStartInteraction (sourcePlayer);

		// Enter the seat
		OnServerEnterSeat ();
		parentEntity.SendMessage ("SeatEnterRequest");
		//RpcEnterSeat (sourcePlayer);

		// Sit animation
		Player p = GameManager.GetPlayerByName (sourcePlayer);
		if (p != null) {
			p.GetComponent<PlayerAnimationController> ().SetSitState (true);
		}
	}

	public override void OnServerExit(string sourcePlayer) {
		// Make sure to call base functions
		base.OnServerExit (sourcePlayer);

		// Enter the seat
		OnServerExitSeat ();
		parentEntity.SendMessage ("SeatExitRequest");
		//RpcExitSeat (sourcePlayer);

		// Sit animation
		Player p = GameManager.GetPlayerByName (sourcePlayer);
		if (p != null) {
			p.GetComponent<PlayerAnimationController> ().SetSitState (false);
		}
	}

	// Called on the server when player enters the seat
	public virtual void OnServerEnterSeat() {

	}

	// Called on the server when player exits the seat
	public virtual void OnServerExitSeat() {

	}
		
	public void EnterSeat(string masterId) {
		isAvailable = false;
		PlayerController controller = GameManager.GetPlayerByName (masterId).GetComponent<PlayerController> ();
		NetworkTransform netTransform = controller.GetComponent<NetworkTransform> ();

		// Disable network position sync
		netTransform.enabled = false;

		// Teleport player
		controller.SetCameraOffset (viewHeight);
		Vector3 offset = Vector3.zero;
		offset += transform.right * sitPosition.x;
		offset += transform.up * sitPosition.y;
		offset += transform.forward * sitPosition.z;

		// Misc
		controller.SetPlayerActive (false);
		controller.TeleportPlayer (transform.position + offset);
		controller.transform.parent = transform;

		// Set Player rotation
		controller.SetCameraRotationY (0);
		controller.transform.localEulerAngles = new Vector3 (controller.transform.localEulerAngles.x, 0, controller.transform.localEulerAngles.z);
	}
		
	// Exit seat
	public virtual void ExitSeat(string masterId) {
		isAvailable = true;
		PlayerController controller = GameManager.GetPlayerByName (masterId).GetComponent<PlayerController> ();
		NetworkTransform netTransform = controller.GetComponent<NetworkTransform> ();

		// Teleport player
		Vector3 offset = Vector3.zero;
		offset += transform.right * exitPosition.x;
		offset += transform.up * exitPosition.y;
		offset += transform.forward * exitPosition.z;

		// Misc
		controller.transform.parent = null;
		controller.TeleportPlayer (transform.position + offset);
		controller.SetCameraRotationY (controller.transform.eulerAngles.y);
		controller.SetPlayerActive (true);

		// Enable network position sync
		netTransform.enabled = true;
	}
}
