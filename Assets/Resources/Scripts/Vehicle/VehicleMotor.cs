using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleMotor : MonoBehaviour {

	public enum TransmissionType {Front, Rear}
	public TransmissionType transmissionType;

	public float maxTorgue = 50f;
	public float acceleration = 5;
	public float curAcceleration = 0;
	public float curSpeed;
	public float brakingTorque = 100f;
	public float deacceleration = 500f;
	public float engineDeacceleration = 5f;
	public float maxSteeringAngle = 55f;
	public float curMaxSteeringAngle;
	public float steerForce = 2f;
	private float curSteerForce;
	public float antiRollForce = 500;
	public float downForce = 5f;
	public float steerReturnSpeed = 5f;
	private float accelerationMultiplier;
	public Vector3 com;
	public bool useRollBar = false;
	public bool useFrontAsRotatingWheels = false;
	[HideInInspector]
	public bool isBraking;
	private bool canZeroBrakingTorque = true;

	[Header ("Gears")]
	public Gear[] gears;

	public int curGear = 1;

	[Header ("UI")]
	public Text speedText;
	public Text accelerationText;
	public Text deaccelerationText;
	public Text curveText;

	// Steering
	private float curSteerAngle;

	// Components
	private VehicleController controller;
	private Rigidbody rig;

	// Wheels
	public WheelCollider[] wheelColliders = new WheelCollider[4];
	public Transform[] wheelMeshes = new Transform[4];

	void Start() {
		rig = GetComponent<Rigidbody>();
		rig.centerOfMass = com;
		curMaxSteeringAngle = maxSteeringAngle;

		// Vehicle substeps
		for (int i = 0; i < wheelColliders.Length; i++) {
			wheelColliders [i].ConfigureVehicleSubsteps (5, 12, 15);
		}

	}

	// UPDATE
	void Update () {
		UpdateWheelMeshPositions ();

		if (Input.GetKeyDown (KeyCode.Space) || Input.GetKeyDown (KeyCode.S)) {
			// TRANSMISSION FRONT
			if (transmissionType == TransmissionType.Rear) {
				wheelColliders[0].brakeTorque = brakingTorque;
				wheelColliders[1].brakeTorque = brakingTorque;
			}
			// TRANSMISSION REAR
			if (transmissionType == TransmissionType.Front) {
				wheelColliders[2].brakeTorque = brakingTorque;
				wheelColliders[3].brakeTorque = brakingTorque;
			}
			isBraking = true;
		}
		if (Input.GetKeyUp (KeyCode.Space) || Input.GetKeyUp (KeyCode.S)) {
			// TRANSMISSION FRONT
			if (transmissionType == TransmissionType.Rear) {
				wheelColliders[0].brakeTorque = 0;
				wheelColliders[1].brakeTorque = 0;
			}
			// TRANSMISSION REAR
			if (transmissionType == TransmissionType.Rear) {
				wheelColliders[2].brakeTorque = 0;
				wheelColliders[3].brakeTorque = 0;
			}
			isBraking = false;
		}

		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			ChangeGear (1);
		}

		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			ChangeGear (-1);
		}
	}

	// FIXED UPDATE
	void FixedUpdate() {

		if (controller != null) {
			Steering ();
		}
		PerformMovement ();
		Braking ();
		Gearing ();
		VehicleUI ();

		// Anti roll
		if (useRollBar) {
			RollBar (wheelColliders [0], wheelColliders [1]);
			RollBar (wheelColliders [2], wheelColliders [3]);
		}
	}


	// STEERING
	void Steering() {
		float steerInput = Input.GetAxisRaw ("Horizontal");

		float steeringMultiplier = (maxSteeringAngle / (gears[curGear].highSpeed / (gears[curGear].highSpeed - curSpeed) * 30));
		steeringMultiplier = Mathf.Clamp (steeringMultiplier, .15f, 1f);
		curSteerAngle -= steerInput * (steerForce);

		// Clamp streering angle value to MinMax
		curSteerAngle = Mathf.Clamp (curSteerAngle, -curMaxSteeringAngle, curMaxSteeringAngle);

		// If player input is ZERO, return steering angle to 0 overtime
		if (steerInput == 0 || (!wheelColliders[0].isGrounded && !wheelColliders[1].isGrounded)) {
			curSteerAngle = Mathf.Lerp (curSteerAngle, 0, steerReturnSpeed * Time.deltaTime);
		}

		// Apply steering
		if (useFrontAsRotatingWheels) {
			wheelColliders [2].steerAngle = curSteerAngle;
			wheelColliders [3].steerAngle = curSteerAngle;
		} else {
			wheelColliders [0].steerAngle = -curSteerAngle;
			wheelColliders [1].steerAngle = -curSteerAngle;
		}
	}

	// MOVEMENT
	void PerformMovement() {
		// Input
		float input = Input.GetAxisRaw ("Vertical");
		float rawInput = Input.GetAxisRaw ("Vertical");

		curAcceleration += input * (acceleration / 100);
		curAcceleration = Mathf.Clamp (curAcceleration, 0, 1);
		input = Mathf.Clamp (input, 0, 1);

		if (rawInput != 0 && !isBraking) {
			canZeroBrakingTorque = true;
		}

		// APPLY FORCE ---->
		if (controller != null && curSpeed < gears[curGear].highSpeed && input > 0) {
			// Front Left
			if (wheelColliders [0].isGrounded) {
				wheelColliders [0].motorTorque = transmissionType == TransmissionType.Front ? (accelerationMultiplier * maxTorgue) : 0;
			}
			// Front Right
			if (wheelColliders [1].isGrounded) {
				wheelColliders [1].motorTorque = transmissionType == TransmissionType.Front ? (accelerationMultiplier * maxTorgue) : 0;
			}

			// Rear Left
			if (wheelColliders [2].isGrounded) {
				wheelColliders [2].motorTorque = transmissionType == TransmissionType.Rear ? (accelerationMultiplier * maxTorgue) : 0;
			}

			// Rear Right
			if (wheelColliders [3].isGrounded) {
				wheelColliders [3].motorTorque = transmissionType == TransmissionType.Rear ? (accelerationMultiplier * maxTorgue) : 0;
			}
		}


		// Deaccelerate motor torque
		if (rawInput == 0 && controller != null) {
			wheelColliders [0].motorTorque = transmissionType == TransmissionType.Rear ? 0 : Mathf.Lerp (wheelColliders [0].brakeTorque, 0, engineDeacceleration * Time.deltaTime);
			wheelColliders [1].motorTorque = transmissionType == TransmissionType.Rear ? 0 : Mathf.Lerp (wheelColliders [1].brakeTorque, 0, engineDeacceleration * Time.deltaTime);
			wheelColliders [2].motorTorque = transmissionType == TransmissionType.Front ? 0 : Mathf.Lerp (wheelColliders [2].brakeTorque, 0, engineDeacceleration * Time.deltaTime);
			wheelColliders [3].motorTorque = transmissionType == TransmissionType.Front ? 0 : Mathf.Lerp (wheelColliders [3].brakeTorque, 0, engineDeacceleration * Time.deltaTime);
		}

		// Deaccelerate vehicle overtime
		if (input == 0 && !isBraking && controller != null) {
			Vector3 deacVel = Vector3.MoveTowards (rig.velocity, Vector3.zero, deacceleration * Time.deltaTime);
			rig.velocity = deacVel;
		}

		if (input == 0) {
			curAcceleration = Mathf.Lerp (curAcceleration, 0, engineDeacceleration * Time.deltaTime);
		}

		// Downforce
		rig.AddForce (-Vector3.up * downForce * rig.mass * Time.deltaTime);
	}

	void Braking() {
		// Zero braking torque after pressing movement key
		if (canZeroBrakingTorque) {
			// TRANSMISSION FRONT
			if (transmissionType == TransmissionType.Rear) {
				wheelColliders[0].brakeTorque = 0;
				wheelColliders[1].brakeTorque = 0;
			}
			// TRANSMISSION REAR
			if (transmissionType == TransmissionType.Rear) {
				wheelColliders[2].brakeTorque = 0;
				wheelColliders[3].brakeTorque = 0;
			}
			canZeroBrakingTorque = false;
		}
	}

	void Gearing() {
		float gearScale = 1 / (gears[curGear].highSpeed / curSpeed);
		accelerationMultiplier = gears[curGear].gearCurve.Evaluate (gearScale);

		// Check for reverse gear
		if (curGear == 0) {
			accelerationMultiplier *= -1;
		}

		if (gearScale > .8f) {
			//ChangeGear (1);
		}
	}

	void ChangeGear (int i) {
		curGear += i;
		curGear = Mathf.Clamp (curGear, 0, gears.Length-1);
	}
	// UPDATE WHEEL POSITIONS
	void UpdateWheelMeshPositions() {
		for (int i = 0; i < wheelMeshes.Length; i++) {
			Quaternion rot;
			Vector3 pos;
			wheelColliders [i].GetWorldPose (out pos, out rot);

			if (wheelMeshes.Length > 0) {
				wheelMeshes [i].position = pos;
				wheelMeshes [i].rotation = rot;
			}
		}
	}

	public void RollBar(WheelCollider wheelL, WheelCollider wheelR) {
		WheelHit hit;
		float travelL = 1f;
		float travelR = 1f;

		bool groundedL = wheelL.GetGroundHit (out hit);
		if (groundedL) {
			travelL = (-wheelL.transform.InverseTransformPoint (hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
		}

		bool groundedR = wheelR.GetGroundHit (out hit);
		if (groundedR) {
			travelR = (-wheelR.transform.InverseTransformPoint (hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
		}

		float antiRoll = (travelL - travelR) * antiRollForce;

		if (groundedL) {
			rig.AddForceAtPosition (wheelL.transform.up * -antiRoll, wheelL.transform.position);
		}

		if (groundedR) {
			rig.AddForceAtPosition (wheelR.transform.up * antiRoll, wheelR.transform.position);
		}
	}

	public void EnableVehicle() {
		//StartCoroutine (DriveVehicle ());
	}

	public void SetController (VehicleController v) {
		controller = v;
	}

	void VehicleUI() {
		// Set the current speed
		curSpeed = rig.velocity.magnitude * 3.6f;
		// UI
		if (speedText != null) {
			speedText.text = curSpeed.ToString ("F0");
		}
		if (accelerationText != null) {
			accelerationText.text = wheelColliders [3].motorTorque.ToString ("F0");
		}
		if (deaccelerationText != null) {
			deaccelerationText.text = wheelColliders [3].motorTorque.ToString ("F0");
		}
		if (curveText != null) {
			curveText.text = accelerationMultiplier.ToString ("F2");
		}
	}

	[System.Serializable]
	public class Gear {
		public AnimationCurve gearCurve;
		public float highSpeed;
		public float lowSpeed;
	}
}
