using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Door : Interactable {

	public float moveSpeed = 1f;
	public bool hasSwitch = false;
	public Vector3 openedPosition;
	private Vector3 closedPosition;
	private bool isOpened = false;
	private bool isOpening = false;

	public override void Start() {
		closedPosition = transform.position;
	}

	public override void OnStartInteraction(string masterId) {
		if (!hasSwitch) {
			CmdMoveDoor ();
		}
	}

	[Command]
	public void CmdMoveDoor() {
		if (!isOpening) {
			isOpened = !isOpened;
			isOpening = true;
			RpcMoveDoor (isOpened);
		}
	}


	[ClientRpc]
	void RpcMoveDoor(bool state) {
		StopCoroutine (MoveDoor (!state));
		StartCoroutine (MoveDoor (state));
	}

	IEnumerator MoveDoor(bool state) {
		Vector3 targetPos = transform.position;
		if (state) {
			targetPos += openedPosition;
		} else {
			targetPos = closedPosition;
		}

		float t = Vector3.Distance (transform.position, targetPos) / moveSpeed;
		while (t > 0) {
			t -= Time.deltaTime;
			transform.position = Vector3.MoveTowards (transform.position, targetPos, moveSpeed * Time.deltaTime);
			yield return null;
		}

		if (isServer) {
			CmdOnDoorFinished ();
		}
	}

	[Command]
	void CmdOnDoorFinished() {
		isOpening = false;
	}
}
