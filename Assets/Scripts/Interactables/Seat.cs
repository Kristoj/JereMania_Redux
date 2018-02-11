using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Seat : Interactable {

	public Vector3 sitPosition;
	public Vector3 exitPosition;
	public float viewHeight = .6f;

	public override void OnStartInteraction(string masterId) {
		base.OnStartInteraction (masterId);
		CmdEnterSeat (masterId);
		if (owner != null) {
			owner.GetComponent<PlayerAnimationController> ().SetSitState (true);
		}
	}

	public override void OnExit(string masterId) {
		if (owner != null) {
			owner.GetComponent<PlayerAnimationController> ().SetSitState (false);
		}
		base.OnExit (masterId);
		CmdExitSeat (masterId);
	}

	[Command]
	public virtual void CmdEnterSeat(string masterId) {
		RpcEnterSeat (masterId);
	}

	[Command]
	public virtual void CmdExitSeat(string masterId) {
		RpcExitSeat (masterId);
	}
		
	[ClientRpc]
	public virtual void RpcExitSeat(string masterId) {
		isAvailable = true;
		PlayerController controller = GameManager.GetCharacter (masterId).GetComponent<PlayerController> ();
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

	[ClientRpc]
	void RpcEnterSeat(string masterId) {
		isAvailable = false;
		PlayerController controller = GameManager.GetCharacter (masterId).GetComponent<PlayerController> ();
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
}
