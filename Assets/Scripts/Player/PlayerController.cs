using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

	[Header ("Movement")]
	/* Frame occuring factors */
	public float gravity  = 20.0f;
	public float friction  = 6f;                // Ground friction

	/* Movement stuff */
	public float moveSpeed = 7.0f;  // Ground move speed
	public float runSpeed = 10f;
	public float speedMultiplier = 1f;
	[HideInInspector]
	public float curSpeed;
	public float moveAcceleration = 14;
	public float runAcceleration = 10;   // Ground accel
	private float curAcceleration;
	public float runDeacceleration = 10;   // Deacceleration that occurs when running on the ground
	public float airAcceleration = 2.0f;  // Air accel
	public float airDeacceleration = 2.0f;    // Deacceleration experienced when opposite strafing
	public float airControl = 0.3f;  // How precise air control is
	public float sideStrafeAcceleration = 50;   // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
	public float sideStrafeSpeed = 1;    // What the max speed to generate when side strafing
	public float jumpSpeed = 8.0f;  // The speed at which the character's up axis gains when hitting jump
	public float moveScale = 1.0f;
	public bool hideCursor = true;
	private bool absoluteGroundMode = true;
	public bool isEnabled = true;
	public bool isActive = true;

	[Header ("Camera")]
	public float mouseSensitivity = 30.0f;
	public float playerViewYOffset = .6f; // The height at which the camera is bound to
	public float crouchViewOffset = .4f;
	public float forwardLean = .2f;
	public float downwardLean = .2f;
	private Transform  playerView;  // Must be a camera;  // Must be a camera
	private float curForwardLean;
	private float curViewOffset;
	private float camRotX;
	private float camRotY;
	//[HideInInspector]
	public Vector2 recoilRot;

	private Player player;
	private CharacterController controller;
	private GunController gunController;
	private PlayerStats playerStats;
	private Vector3 playerVelocity = Vector3.zero;
	private Vector3 hardGroundPoint;
	public LayerMask hardGroundLayer;

	// Q3: players can queue the next jump just before he hits the ground
	private bool wishJump = false;
	private bool canJump = true;
	private bool jumpIssued = false;
	public bool isSliding = false;
	public bool canSlide = false;
	public float xEuler;
	public float zEuler;
	[HideInInspector]
	public bool canRun = true;

	// TEMP
	private float targetFov = 90;

	// UI
	public Text velocityText;


	//Contains the command the user wishes upon the character
	class Cmd {
		public float forwardmove;
		public float rightmove;
	}
	private Cmd cmd  ; // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)



	void Start() {
		player = GetComponent<Player>();
		controller = GetComponent<CharacterController> ();
		gunController = GetComponent<GunController> ();
		playerStats = GetComponent<PlayerStats> ();
		curViewOffset = playerViewYOffset;
		curSpeed = moveSpeed;
		curAcceleration = moveAcceleration;
		playerView = player.cam.transform;
		camRotY = transform.eulerAngles.y;
		Application.targetFrameRate = 60;

		/* Hide the cursor */
		if (hideCursor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		if (IsGrounded ()) {
			StartCoroutine (OnPlayerLand ());
		} else {
			StartCoroutine (OnPlayerAirborne ());
		}

		cmd = new Cmd();
	}

	void Update() {
		CursorStateCheck ();
		if (isEnabled) {
			CameraRotation ();
		}
		/* Movement, here's the important part */

		if (isActive && isEnabled) {
			QueueJump ();
			if (controller.isGrounded)
				GroundMove ();
			else if (!controller.isGrounded) {
				AirMove ();
			}
			Move (playerVelocity * Time.deltaTime);
		}
		CheckPlayerInput ();
	}

	/*******************************************************************************************************\
|* MOVEMENT
\*******************************************************************************************************/

	/**
	* Sets the movement direction based on player input
	*/
	void SetMovementDir() {
		cmd.forwardmove = Input.GetAxisRaw("Vertical");
		cmd.rightmove = Input.GetAxisRaw("Horizontal");
	}

	/**
     * Queues the next jump just like in Q3
     */
	void QueueJump() {
		if ((Input.GetKeyDown (KeyCode.Space) || Input.GetAxisRaw ("Mouse ScrollWheel") < 0) && !wishJump && canJump) {
			wishJump = true;
			jumpIssued = true;
			StartCoroutine ("JumpQueueReset");
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			wishJump = false;
			jumpIssued = false;
		}
	}

	// Jump queueing
	IEnumerator JumpQueueReset() {
		yield return new WaitForSeconds (.001f);
		wishJump = false;
	}

	// Called when player is no longer grounded
	IEnumerator OnPlayerAirborne() {
		float heightPeak = transform.position.y;
		if (jumpIssued) {
			//canGroundStrafe = false;
			absoluteGroundMode = false;
			canJump = false;
			StartCoroutine (EnableJump ());
		}

		yield return new WaitForSeconds (.02f);

		// While airborne
		while (!IsGrounded()) {
			// Get the highest peak in the jump
			if (transform.position.y > heightPeak) {
				heightPeak = transform.position.y;
			}
			yield return null;
		}

		if (Mathf.Abs ((transform.position.y - heightPeak)) > .09f && transform.position.y < heightPeak && !IsHardGrounded()) {
			AudioManager.instance.CmdPlaySound2D ("Jump_Land1", transform.position, transform.name, .35f);
			gunController.AddSway (new Vector3 (0, -.5f, 0));
		}

		jumpIssued = false;
		StartCoroutine (SetPlayerAbsoluteGroundMode());
		StartCoroutine (EnableGroundStrafe());
		StartCoroutine (OnPlayerLand ());
	}

	// Called when player lands on the ground
	IEnumerator OnPlayerLand() {
		while (IsGrounded()) {
			yield return null;
		}
		StartCoroutine (OnPlayerAirborne ());
	}

	IEnumerator SetPlayerAbsoluteGroundMode() {
		yield return new WaitForSeconds (.8f);
		if (controller.isGrounded) {
			absoluteGroundMode = true;
		}
	}

	IEnumerator EnableGroundStrafe() {
		yield return new WaitForSeconds (.08f);

		if (IsGrounded()) {
			//canGroundStrafe = true;
		}
	}

	IEnumerator EnableJump() {
		yield return new WaitForSeconds (.1f);
		canJump = true;
	}

	/**
     * Execs when the player is in the air
     */
	void AirMove() {
		Vector3 wishdir;
		float accel;

		float scale = CmdScale();

		SetMovementDir();

		wishdir = new  Vector3(cmd.rightmove, 0f, cmd.forwardmove);
		wishdir = transform.TransformDirection(wishdir);

		float wishspeed = wishdir.magnitude;
		wishspeed *= curSpeed * speedMultiplier;

		wishdir.Normalize();
		wishspeed *= scale;

		//CPM: Aircontrol
		float wishspeed2 = wishspeed;
		if (Vector3.Dot (playerVelocity, wishdir) < -5) {
			accel = airDeacceleration;
		} else {
			//accel = airAcceleration;
			accel = airDeacceleration;
		}
		//If the player is ONLY strafing left or right
		if (cmd.forwardmove == 0 && cmd.rightmove != 0) {
			if (wishspeed > sideStrafeSpeed) {
				//wishspeed = sideStrafeSpeed;
			}
			//accel = sideStrafeAcceleration;
		}

		Accelerate(wishdir, wishspeed, accel);
		if (airControl > 0)
			AirControl(wishdir, wishspeed2);
		// !CPM: Aircontrol

		playerVelocity.y -= gravity * Time.deltaTime;

		//LEGACY MOVEMENT SEE BOTTOM
	}

	/**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
	void AirControl(Vector3 wishdir, float wishspeed) {
		float zspeed;
		float speed;
		float dot;
		float k;

		//Can't control movement if not moving forward or backward
		if (cmd.forwardmove == 0 || wishspeed == 0) {
			return;
		}

		zspeed = playerVelocity.y;
		playerVelocity.y = 0;
		/* Next two lines are equivalent to idTech's VectorNormalize() */
		speed = playerVelocity.magnitude;
		playerVelocity.Normalize();

		dot = Vector3.Dot(playerVelocity, wishdir);
		k = 32;
		k *= airControl * dot * dot * Time.deltaTime;

		// Change direction while slowing down
		if (dot > 0) {
			playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
			playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
			playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

			playerVelocity.Normalize();
		}

		playerVelocity.x *= speed;
		playerVelocity.y = zspeed; // Note this line
		playerVelocity.z *= speed;


	}

	/**
     * Called every frame when the engine detects that the player is on the ground
     */
	void GroundMove() {
		Vector3 wishdir;

		// Do not apply friction if the player is queueing up the next jump
		if (!wishJump) {
			ApplyFriction (1);
		} else {
			ApplyFriction (0);
		}

		SetMovementDir();

		wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);



		wishdir = transform.TransformDirection(wishdir);
		wishdir.Normalize();

		var wishspeed = wishdir.magnitude;
		wishspeed *= curSpeed * speedMultiplier;

		// Make sure that when bunny hop landing on the ground player doesn't strafe
		/**if (!canGroundStrafe) {
			wishspeed = 0;
		}**/

		//if (canGroundStrafe) {
		Accelerate (wishdir, wishspeed, curAcceleration);
		//}

		Vector3 vel = controller.velocity;
		vel.y = 0;
		playerVelocity.y = 0;


		if (absoluteGroundMode) {
			playerVelocity.x = Mathf.Clamp (playerVelocity.x, -curSpeed, curSpeed);
			playerVelocity.z = Mathf.Clamp (playerVelocity.z, -curSpeed, curSpeed);
		}

		if (wishJump) {
			playerVelocity.y = jumpSpeed;
			wishJump = false;
			AudioManager.instance.CmdPlaySound2D ("Jump_Vocal", transform.position, transform.name, .5f);
			AudioManager.instance.CmdPlaySound2D ("Jump_Start1", transform.position, transform.name, .5f);
			gunController.AddSway (new Vector3 (0, -.5f, 0));
			playerStats.StaminaRemove (18f, true);
			playerStats.FatiqueRemove (.1f);
		}
	}

	/**
     * Applies friction to the player, called in both the air and on the ground
     */
	void ApplyFriction(float t) {
		Vector3 vec  = playerVelocity; // Equivalent to: VectorCopy();
		float speed ;
		float newspeed ;
		float control ;
		float drop ;

		vec.y = 0.0f;
		speed = vec.magnitude;
		drop = 0.0f;

		/* Only if the player is on the ground then apply friction */
		if (controller.isGrounded) {
			control = speed < runDeacceleration ? runDeacceleration : speed;
			drop = control * friction * Time.deltaTime * t;
		}

		newspeed = speed - drop;
		if (newspeed < 0)
			newspeed = 0;
		if (speed > 0)
			newspeed /= speed;

		playerVelocity.x *= newspeed;
		playerVelocity.y *= newspeed;
		playerVelocity.z *= newspeed;
	}

	/**
     * Calculates wish acceleration based on player's cmd wishes
     */
	void Accelerate(Vector3 wishdir , float wishspeed, float accel) {
		float addspeed;
		float accelspeed;
		float currentspeed;

		currentspeed = Vector3.Dot(playerVelocity, wishdir);
		addspeed = wishspeed - currentspeed;
		if (addspeed <= 0) {
			return;
		}
		accelspeed = accel * Time.deltaTime * wishspeed;
		if (accelspeed > addspeed) {
			accelspeed = addspeed;
		}

		playerVelocity.x += accelspeed * wishdir.x;
		playerVelocity.z += accelspeed * wishdir.z;
	}

	// Rotate player camera
	void CameraRotation() {
		/* Camera rotation stuff, mouse controls this shit */
		camRotX -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * 0.02f;
		camRotY += Input.GetAxisRaw("Mouse X") * mouseSensitivity * 0.02f;

		//Clamp the X rotation
		if (camRotX < -90)
			camRotX = -90;
		else if (camRotX > 90)
			camRotX = 90;

		Vector3 parentEuler = Vector3.zero;
		if (transform.parent != null) {
			parentEuler = transform.parent.eulerAngles;
		}

		this.transform.rotation = Quaternion.Euler(new Vector3 (0, camRotY + recoilRot.x, 0) + parentEuler); // Rotates the collider
		playerView.rotation = Quaternion.Euler(new Vector3 (camRotX + recoilRot.y, camRotY + recoilRot.x, 0) + parentEuler); // Rotates the camera

		//Set the camera's position to the transform
		//playerView.position = this.transform.position;
		//playerView.position.y = this.transform.position.y + playerViewYOffset;
		float leanPercentage = camRotX / 90;
		float ySubtraction = downwardLean * leanPercentage;
		Vector3 targetPos = (transform.up * (curViewOffset - ySubtraction)) + (transform.forward * forwardLean * leanPercentage);
		playerView.position = transform.position + targetPos;
	}

	/*
    ============
    PM_CmdScale
    Returns the scale factor to apply to cmd movements
    This allows the clients to use axial -127 to 127 values for all directions
    without getting a sqrt(2) distortion in speed.
    ============
    */
	float CmdScale() {
		float max = 0;
		float total;
		float scale;

		max = Mathf.Abs(cmd.forwardmove);
		if (Mathf.Abs (cmd.rightmove) > max) {
			max = Mathf.Abs (cmd.rightmove);
		}
		if (max == 0) {
			return 0;
		}

		total = Mathf.Sqrt(cmd.forwardmove * cmd.forwardmove + cmd.rightmove * cmd.rightmove);
		scale = curSpeed * max / (moveScale * total);

		return scale;
	}

	void CheckPlayerInput() {

		// Crouching
		if (Input.GetKeyDown (KeyCode.LeftControl) && isActive && isEnabled) {
			curViewOffset = crouchViewOffset;
		}

		if (Input.GetKeyUp (KeyCode.LeftControl) && isActive && isEnabled) {
			curViewOffset = playerViewYOffset;
		}

		// Running
		if (Input.GetKeyDown (KeyCode.LeftShift) && isActive && isEnabled && cmd.forwardmove > 0 && canRun) {
			curSpeed = runSpeed;
			curAcceleration = runAcceleration;
			targetFov = 105;
			playerStats.StaminaDrain (true);
		}

		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			curSpeed = moveSpeed;
			playerStats.StaminaDrain (false);
		}

		if (playerStats.stamina <= 0) {
			curSpeed = moveSpeed;
		}

		if (curSpeed == moveSpeed) {
			targetFov = 90;
			curAcceleration = moveAcceleration;
		}

		if (cmd.forwardmove <= 0) {
			curSpeed = moveSpeed;
		}

		player.cam.fieldOfView = Mathf.Lerp (player.cam.fieldOfView, targetFov, 6 * Time.deltaTime);
	}

	void CursorStateCheck() {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		/* Ensure that the cursor is locked into the screen */
		if (Cursor.lockState == CursorLockMode.None)
		{
			if (Input.GetMouseButtonDown(0) && isEnabled)
				Cursor.lockState = CursorLockMode.Locked;
		}
	}

	public bool IsGrounded() {
		Vector3 ray = transform.position + controller.center;
		RaycastHit hit;


		if (Physics.SphereCast (ray, controller.height / 2, -Vector3.up, out hit, .34f, hardGroundLayer, QueryTriggerInteraction.Ignore)) {
			return true;
		} else {
			return false;
		}
	}

	public bool IsHardGrounded() {
		Ray ray = new Ray (transform.position, -Vector3.up);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, controller.bounds.size.y / 2 + .25f, hardGroundLayer, QueryTriggerInteraction.Ignore)) {
			hardGroundPoint.y = hit.point.y;
			return true;
		} else {
			return false;
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		float hitAngle = Vector3.Angle (hit.normal, transform.up);
		if (hitAngle > 45) {
			if (!isSliding) {
				StartCoroutine (Slide ());
			}
			canSlide = true;
		} else {
			canSlide = false;
		}
	}

	IEnumerator Slide() {

		float t = 0;
		float slideTime = .5f;
		isSliding = true;

		// While sliding
		while (t < slideTime) {
			t += Time.deltaTime;
			if (canSlide) {
				t = 0;
			}
			yield return null;
		}
		isSliding = false;
	}

	public void SetPlayerActive (bool state) {
		if (state) {
			isActive = true;
			controller.GetComponent<CharacterController> ().enabled = true;
			curViewOffset = playerViewYOffset;
		} else {
			isActive = false;
			controller.GetComponent<CharacterController> ().enabled = false;
		}
	}

	public void SetPlayerEnabled(bool state) {
		if (state) {
			isEnabled = true;
		} else {
			isEnabled = false;
		}
	}

	void Move(Vector3 vel) {
		controller.Move (vel);


		// Move the controller
		if (IsHardGrounded () && !wishJump && controller.velocity.y <= 0) {
			playerVelocity.y = 0;
			controller.transform.position = new Vector3 (controller.transform.position.x, hardGroundPoint.y + controller.skinWidth + (controller.height / 2), controller.transform.position.z);
		}
	}

	public void SetCameraOffset (float f) {
		curViewOffset = f;
	}
		
	public void TeleportPlayer(Vector3 pos) {
		transform.position = pos;
	}

	public void SetCameraRotationY (float yRot) {
		camRotY = yRot;
	}

	public void AddCameraRotationX (float xRot) {
		camRotX += xRot;
	}
}