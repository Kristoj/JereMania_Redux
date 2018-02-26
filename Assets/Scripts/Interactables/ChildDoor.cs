using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildDoor : ChildInteractable {

	public float doorSwingTime = 2.5f;
	public bool isClosed = true;
	public bool isMoving = false;
	// Door rotations
	public Vector3 openEuler;
	public Vector3 openPos;
	private Vector3 closedEuler;
	private Vector3 closedPos;
	private Vector3 curDoorEuler;
	private Coroutine swingCoroutine;

	void Start() {
		closedEuler = transform.localEulerAngles;
		curDoorEuler = transform.localEulerAngles;
		closedPos = transform.localPosition;

		if (openPos == Vector3.zero) {
			openPos = transform.localPosition;
		}
	}

	public override void OnServerStartInteraction(string masterId) {
		if (parentEntity != null) {
			parentEntity.SendMessage ("SignalDoorSwing", transform.name, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SwingDoor(int swingDir) {
		if (isMoving) {
			return;
		}

		if (swingCoroutine != null) {
			StopCoroutine (swingCoroutine);
		}
		swingCoroutine = StartCoroutine (DoorSwing (swingDir));
	}

	IEnumerator DoorSwing(int swingDir) {
		Vector3 targetEuler;
		Vector3 targetPos;

		if (swingDir == 1) {
			targetEuler = openEuler;
			targetPos = openPos;
		} else {
			targetEuler = closedEuler;
			targetPos = closedPos;
		}
		float t = 0;
		while (t < 1) {
			// Increment time overtime
			t += Time.deltaTime / doorSwingTime;
			// Position lerp
			transform.localPosition = Vector3.Lerp (transform.localPosition, targetPos, t);
			// Rotation lerp
			curDoorEuler.x = Mathf.LerpAngle (curDoorEuler.x, targetEuler.x, t);
			curDoorEuler.y = Mathf.LerpAngle (curDoorEuler.y, targetEuler.y, t);
			curDoorEuler.z = Mathf.LerpAngle (curDoorEuler.z, targetEuler.z, t);
			transform.localEulerAngles = curDoorEuler;
			yield return null;
		}
	}
}
