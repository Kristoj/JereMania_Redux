using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VehicleController : Seat {

	[Header ("Movement Sync")]
	public float posSyncRate = 15;
	public float posInterpolateSpeed = 5f;
	private float positionInterval;
	[SyncVar]
	private Vector3 targetPos;

	[Header ("Rotation Sync")]
	public float rotSyncRate = 15;
	public float rotInterpolateSpeed = 5f;
	private float rotationInterval;
	[SyncVar]
	private Vector3 targetRot;

	// Classes
	public VehicleMotor motor;

	// Components
	[HideInInspector]
	public bool isDriving;

	public override void Start() {
		rig = motor.GetComponent<Rigidbody> ();
		motor.enabled = false;
		positionInterval = 1 / posSyncRate;

		if (isServer) {
			targetPos = rig.position;
			targetRot = rig.rotation.eulerAngles;
		}
	}

	void FixedUpdate () {


		if (!isDriving) {
			rig.position = Vector3.Lerp (rig.position, targetPos, posInterpolateSpeed * Time.deltaTime);
			// Rotation
			Vector3 newRot;
			newRot.x = Mathf.LerpAngle (rig.rotation.eulerAngles.x, targetRot.x, rotInterpolateSpeed * Time.deltaTime);
			newRot.y = Mathf.LerpAngle (rig.rotation.eulerAngles.y, targetRot.y, rotInterpolateSpeed * Time.deltaTime);
			newRot.z = Mathf.LerpAngle (rig.rotation.eulerAngles.z, targetRot.z, rotInterpolateSpeed * Time.deltaTime);
			rig.rotation = Quaternion.Euler (newRot);
		}


	}

	public void OnVehicleDisable() {
		isDriving = false;
		motor.enabled = false;
	}

	public IEnumerator OnVehicleEnable() {
		isDriving = true;
		motor.enabled = true;
		DriveCar ();

		while (isDriving) {
			targetPos = rig.position;
			targetRot = rig.rotation.eulerAngles;
			CmdSetTargetData (targetPos, targetRot);
			yield return new WaitForSeconds (positionInterval);
		}
	}

	void DriveCar() {
		motor.SetController (this);
		motor.EnableVehicle ();
	}

	[Command]
	void CmdSetTargetData(Vector3 pos, Vector3 rot) {
		targetPos = pos;
		targetRot = rot;
	}

	public override void OnStartInteraction(string masterId) {
		base.OnStartInteraction (masterId);
		StartCoroutine(OnVehicleEnable());

		// Disable view model
		owner.GetComponent<PlayerAnimationController>().EnableViewModel (false);
	}

	public override void OnExit(string masterId) {
		// Enable view model
		if (owner != null) {
			owner.GetComponent<PlayerAnimationController> ().EnableViewModel (true);
		} else {
			Debug.Log ("Owner is NULL when exiting seat!!!");
		}

		base.OnExit (masterId);
		OnVehicleDisable ();
	}
}
