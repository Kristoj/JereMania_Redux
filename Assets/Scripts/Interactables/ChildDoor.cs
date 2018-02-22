using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class ChildDoor : ChildInteractable {

	public float doorOpenTime = 1f;
	private bool isClosed;
	private bool isMoving = false;
	// Door rotations
	public Vector3 openEuler;
	private Vector3 closedEuler;
	private Vector3 curDoorEuler;

	// Components
	private Rigidbody rig;

	public override void Start() {
		base.Start ();
		rig = GetComponent<Rigidbody> ();
		closedEuler = transform.rotation.eulerAngles;
	}

	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);
		RequestDoorOpen ();
	}

	void RequestDoorOpen() {
		if (parentEntity != null) {
			parentEntity.SendMessage ("SignalDoorOpen", transform.name, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OpenDoor() {
		if (isMoving) {
			return;
		}
		StartCoroutine (DoorSwing ());
	}

	IEnumerator DoorSwing() {
		isMoving = true;
		Vector3 targetEuler;
		if (isClosed) {
			targetEuler = openEuler;
		} else {
			targetEuler = closedEuler;
		}
		Debug.Log ("Swing");
		float t = doorOpenTime;
		while (t < 1) {
			t += Time.deltaTime / doorOpenTime;
			curDoorEuler = Vector3.Lerp (curDoorEuler, targetEuler, t);
			yield return null;
		}
		isMoving = false;

		if (!isClosed) {
			isClosed = true;
		} else {
			isClosed = false;
		}

		Quaternion newRot = Quaternion.Euler (curDoorEuler);
		rig.rotation = newRot;
	}
}
