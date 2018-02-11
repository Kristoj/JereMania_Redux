using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class GunController : NetworkBehaviour {

	public Transform gunHoldR;
	public Transform gunHoldL;
	public Transform weaponHolsterR;
	public Transform weaponHolsterL;
	public Transform serverGunHold;
	public Transform armHolder;
	public Weapon weapon01;
	public Weapon weapon02;
	public Weapon weapon03;
	public LayerMask hitMask;

	//[HideInInspector]
	public Equipment currentEquipment;
	private Player player;
	[HideInInspector]
	public PlayerAnimationController animController;
	private PlayerController playerController;
	private CharacterController controller;
	//private PlayerUI playerUI;

	[Header ("Sway")]
	public Vector3 weaponOffset;
	public Vector3 aimOffset;
	private Vector3 curOffset;
	public float swayScaleX = .2f;
	public float swaySpeedX = 3f;
	public float swayReturnSpeedX = 5f;
	public float swayScaleY = .2f;
	public float swaySpeedY = 3f;
	public float swayReturnSpeedY = 5f;
	public float swayScaleZ = .2f;
	public float swaySpeedZ = 3f;
	public float swayReturnSpeedZ = 5f;
	private Vector3 originalViewPos;
	private Vector3 swayPosition;

	[Header ("Bobbing")]
	public float bobSpeed = 1f;
	public float bobSmooth = 20f;
	public float horizontalRange = .1f;
	public float verticalRange = .1f;
	public AudioClip[] footStepSounds;
	private float curHorizontalRange;
	private float curVerticalRange;
	private bool hPositive = true;
	private bool vPositive = true;
	private Vector3 headOriginalPos;
	private float percentage;
	private Vector3 bobPos;
	private Vector3 bobVector;
	private Vector3 kickPosVector;
	private Vector2 targetRecoilVector;
	private Vector2 curRecoilVector;


	// Misc
	//[HideInInspector]
	public bool isChargingSecondaryAction = false;
	public bool canChangeEquipment = true;
	public bool isAttacking = false;
	public bool canAttack = true;

	// Button states
	[HideInInspector]
	public bool mouseLeftDown;
	[HideInInspector]
	public bool mouseRightDown;
	[HideInInspector]
	public bool mouseLeftReleased;
	[HideInInspector]
	public bool mouseRightReleased;

	// Use this for initialization
	void Start () {
		mouseLeftReleased = true;
		mouseRightReleased = true;
		player = GetComponent<Player> ();
		animController = GetComponent<PlayerAnimationController> ();
		playerController = GetComponent<PlayerController> ();
		controller = GetComponent<CharacterController> ();
		//playerUI = GetComponent<PlayerUI> ();
		kickPosVector = Vector3.zero;


		// Get GunHolds
		gunHoldR = transform.Find ("Main Camera").transform.Find("Arm Holder").transform.Find("View_Model").transform.Find("Game_engine").transform.Find("spine_03").transform.Find("clavicle_r").transform.Find("upperarm_r").
			transform.Find("lowerarm_r").transform.Find("hand_r").transform.Find("gunHold.R").transform;
		gunHoldL = transform.Find ("Main Camera").transform.Find("Arm Holder").transform.Find("View_Model").transform.Find("Game_engine").transform.Find("spine_03").transform.Find("clavicle_l").transform.Find("upperarm_l").
			transform.Find("lowerarm_l").transform.Find("hand_l").transform.Find("gunHold.L").transform;
		// Setup weapon hold models
		UpdateWeaponHolster();
		if (armHolder != null) {
			originalViewPos = armHolder.transform.localPosition;
		}

		if (!isLocalPlayer) {
			return;
		}

		if (weapon01 != null) {
			EquipEquipment (weapon01, 1);
		}
	}

	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) {
			return;
		}

		CheckMouseButtonStates ();

		// Shooting
		if (Input.GetButton ("Fire1")) {
			if (isLocalPlayer) {
				ShootPrimary ();
				mouseLeftReleased = false;
			}
		}

		// Shooting
		if (Input.GetButtonDown ("Fire2")) {
			if (isLocalPlayer && currentEquipment != null) {
				//Aim ();
				currentEquipment.OnSecondaryAction();
			}
		}

		// Shooting
		if (Input.GetKeyDown (KeyCode.G)) {
			if (isLocalPlayer && currentEquipment != null && currentEquipment.objectName != "Hammer" && !isAttacking) {
				EquipEquipment (EquipmentLibrary.instance.GetEquipment ("Hatchet"), 1f);
			}
		}

		if (animController.viewModelEnabled) {
			Sway ();
			AnimateBob ();
		}

		// Weapon Equipping
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			if (weapon01 != null && !isAttacking) {
				EquipEquipment (weapon01, 1);
			}
		}

		// Weapon Equipping
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			if (weapon02 != null && !isAttacking) {
				EquipEquipment (weapon02, 1);
			}
		}

		// Weapon Equipping
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			if (weapon03 != null && !isAttacking) {
				EquipEquipment (weapon03, 1);
			}
		}

		// Recoil Lerp
		/**
		curRecoilVector.x = Mathf.Lerp (curRecoilVector.x, targetRecoilVector.x, currentWeapon.recoilSpeedX * Time.deltaTime);
		curRecoilVector.y = Mathf.Lerp (curRecoilVector.y, targetRecoilVector.y, currentWeapon.recoilSpeedY * Time.deltaTime);
		playerController.recoilRot = curRecoilVector;
		if (!mouseLeftDown && currentWeapon.fireMode == Gun.FireMode.Auto) {
			targetRecoilVector = Vector2.Lerp (targetRecoilVector, Vector2.zero, currentWeapon.recoilReturnSpeed * Time.deltaTime);
		} else if (currentWeapon.fireMode == Gun.FireMode.Semi){
			targetRecoilVector = Vector2.Lerp (targetRecoilVector, Vector2.zero, currentWeapon.recoilReturnSpeed * Time.deltaTime);
		}
		**/
	}


	void ShootPrimary() {
		if (currentEquipment != null && playerController.isActive && playerController.isEnabled) {
			currentEquipment.OnPrimaryAction ();
		}
	}

	public void EquipEquipment(Equipment equipmentToEquip, float dropForce) {
		if (!canChangeEquipment) {
			return;
		}

		canChangeEquipment = false;
		canAttack = false;

		// Animation
		if (equipmentToEquip.objectName == weapon01.objectName || equipmentToEquip.objectName == weapon02.objectName || equipmentToEquip.objectName == weapon03.objectName) {
			animController.ChangeWeapon ();
		}
			
		StartCoroutine (DropEquipment (equipmentToEquip, dropForce));
	}

	IEnumerator DropEquipment(Equipment equipmentToEquip, float dropForce) {

		// Destroy current weapon if one excists
		if (currentEquipment != null) {
			if (currentEquipment.objectName != weapon01.objectName && currentEquipment.objectName != weapon02.objectName && currentEquipment.objectName != weapon03.objectName) {
				// Raycast drop direction
				Ray ray = new Ray (player.cam.transform.position, player.cam.transform.forward);
				RaycastHit hit; 
				if (Physics.Raycast (ray, out hit, 500, hitMask)) {
					Vector3 dropDir = ((hit.point + -(transform.right * .6f)) - player.cam.transform.position).normalized;
					//dropDir.y
					currentEquipment.CmdDropItem (currentEquipment.objectName, transform.name, gunHoldR.position, gunHoldR.rotation, dropDir, dropForce);
				} else {
					currentEquipment.CmdDropItem (currentEquipment.objectName, transform.name, gunHoldR.position, gunHoldR.rotation, player.cam.transform.forward, dropForce);
				}
			}
			CmdDestroyCurrentEquipment ();
		}
		yield return new WaitForSeconds (.15f);
		// Spawn new gun
		CmdSpawnEquipment (equipmentToEquip.objectName, netId);
	}

	public void DestroyCurrentEquipment() {
		canChangeEquipment = false;
		canAttack = false;

		animController.ChangeWeapon ();
		CmdDestroyCurrentEquipment ();
		CmdSpawnEquipment (weapon01.objectName, netId);
	}

	[Command]
	void CmdDestroyCurrentEquipment() {
		NetworkServer.Destroy (currentEquipment.gameObject);
	}

	[Command]
	void CmdSpawnEquipment (string _name, NetworkInstanceId ownerId) {

		Equipment equipmentRef = EquipmentLibrary.instance.GetEquipment (_name);
		Equipment clone = Instantiate (equipmentRef, serverGunHold.position, serverGunHold.rotation, serverGunHold.transform) as Equipment;
		clone.GetComponent<Rigidbody> ().isKinematic = true;
		currentEquipment = clone;
		currentEquipment.gameObject.layer = LayerMask.NameToLayer ("LocalPlayer");
		currentEquipment.isAvailable = false;
		NetworkServer.Spawn (clone.gameObject);
		RpcSpawnEquipment (clone.netId, ownerId);

		player = GetComponent<Player> ();
		var msg = new MyMessage();
		msg.message = _name;
		msg.playerId = netId;
		msg.objectId = clone.netId;


		base.connectionToClient.Send(player.myMsgId, msg);
	}

	[ClientRpc]
	void RpcSpawnEquipment (NetworkInstanceId cloneId, NetworkInstanceId ownerId) {
		// Do stuff for the equipment on other clients
		GameObject localEquipment = ClientScene.FindLocalObject (cloneId);
		GameObject ownerPlayer = ClientScene.FindLocalObject (ownerId);
		if (localEquipment != null) {
			Rigidbody cloneRig = localEquipment.GetComponent<Rigidbody> ();
			Transform gunHold = ownerPlayer.GetComponent<GunController> ().serverGunHold;
			if (cloneRig != null) {
				cloneRig.isKinematic = true;
			}

			if (gunHold != null) {
				cloneRig.transform.parent = gunHold.transform;
				cloneRig.transform.position = gunHold.transform.position;
				cloneRig.transform.rotation = gunHold.transform.rotation;
			}
		}
	}

	public void ClientSpawnWeapon(NetworkInstanceId gunObjecetId, string owner) {

		// Spawn new equipment
		currentEquipment = ClientScene.FindLocalObject (gunObjecetId).GetComponent<Equipment>();
		currentEquipment.SetOwner (transform, owner);
		currentEquipment.GetComponent<Rigidbody> ().isKinematic = true;

		// Move and rotate new equipment
		currentEquipment.transform.parent = gunHoldR.transform;
		currentEquipment.transform.localPosition = Vector3.zero;
		currentEquipment.transform.localRotation = Quaternion.identity;
		bobPos = Vector3.zero;
		canAttack = true;
		isAttacking = false;

		// Stop sprinting if current equipment doesn't allow it
		if (!currentEquipment.canRunWith) {
			playerController.curSpeed = playerController.moveSpeed;
		}

		// Equipment offset and animations and visuals
		weaponOffset = currentEquipment.positionOffset;
		curOffset = weaponOffset;
		animController.SetGunAnimationIds (currentEquipment.GetAnimationIds());
		UpdateWeaponHolster ();

		// Set layers
		currentEquipment.gameObject.layer = LayerMask.NameToLayer ("LocalPlayer");
		if (currentEquipment.transform.childCount > 0) {
			for (int i = 0; i < currentEquipment.transform.childCount; i++) {
				currentEquipment.transform.GetChild (i).gameObject.layer = LayerMask.NameToLayer ("LocalPlayer");
			}
		}
		// Misc
		currentEquipment.SetHitMask ();
		playerController.speedMultiplier = currentEquipment.speedMultiplier;
		playerController.canRun = currentEquipment.canRunWith;

		// Wait awhile after spawning new weapon before allowing actions with it
		StartCoroutine (EquipmentChangeReset());
	}

	IEnumerator EquipmentChangeReset() {
		yield return new WaitForSeconds (.1f);
		canChangeEquipment = true;
		canAttack = true;
		isAttacking = false;
	}

	public void OnSpawnNewWeapon(NetworkMessage netMsg) {

		var msg = netMsg.ReadMessage<MyMessage>();
		PlayerInteraction pi = GetComponent<PlayerInteraction> ();
		// INTERA
		if (msg.message == "Interact") {

			if (pi.targetIntera != null) {
				pi.targetIntera.OnStartInteraction (transform.name);
			}
		} else if (msg.message == "Pickup") {
			Equipment qe = pi.targetIntera.GetComponent<Equipment> ();
			if (qe != null) {
				qe.SetOwner (transform, transform.name);
				pi.targetIntera.OnStartPickup (transform.name);
			}
		} else {
			GunController gunController = ClientScene.FindLocalObject(msg.playerId).GetComponent<GunController>();
			gunController.ClientSpawnWeapon (msg.objectId, gunController.gameObject.name);
		}
	}

	void UpdateWeaponHolster() {

		if (weaponHolsterL != null && weaponHolsterR != null) {

			if (weaponHolsterL.transform.childCount > 0) {
				Destroy (weaponHolsterL.GetChild (0).gameObject);
			}
			if (weaponHolsterR.transform.childCount > 0) {
				Destroy (weaponHolsterR.GetChild (0).gameObject);
			}

			if (currentEquipment == null) {
				return;
			}

			if (currentEquipment.objectName == weapon03.objectName || currentEquipment.objectName == weapon01.objectName) {
				Equipment cloneL = Instantiate (weapon02, weaponHolsterL.position, weaponHolsterL.rotation) as Equipment;
				cloneL.enabled = false;
				cloneL.transform.localScale = weaponHolsterL.transform.localScale;
				cloneL.transform.SetParent (weaponHolsterL.transform);
				cloneL.GetComponent<Rigidbody> ().isKinematic = true;
			}
			if (currentEquipment.objectName == weapon02.objectName || currentEquipment.objectName == weapon01.objectName) {
				Equipment cloneR = Instantiate (weapon03, weaponHolsterR.position, weaponHolsterR.rotation) as Equipment;
				cloneR.enabled = false;
				cloneR.transform.localScale = weaponHolsterR.transform.localScale;
				cloneR.transform.SetParent (weaponHolsterR.transform);
				cloneR.GetComponent<Rigidbody> ().isKinematic = true;
			}
		}
	}

	public void CheckMouseButtonStates() {
		if (Input.GetButtonDown ("Fire1")) {
			mouseLeftDown = true;
		}

		if (Input.GetButtonUp ("Fire1")) {
			mouseLeftDown = false;
			mouseLeftReleased = true;
			isChargingSecondaryAction = false;
		}

		if (Input.GetButtonDown ("Fire2")) {
			mouseRightDown = true;
			isChargingSecondaryAction = true;
			canAttack = false;
		}

		if (Input.GetButtonUp ("Fire2")) {
			mouseRightDown = false;
			mouseRightReleased = true;
			isChargingSecondaryAction = false;
		}
	}

	void Sway() {
		if (armHolder != null) {

			// Input
			float xInput = Input.GetAxis ("Mouse X");
			float yInput = Input.GetAxis ("Mouse Y");
			float zInput = Input.GetAxisRaw ("Vertical");

			float speedMultiplier = 1f;
			float aimMoveMultiplier = 1f;
			if (isChargingSecondaryAction) {
				speedMultiplier = .1f;
				aimMoveMultiplier = 2f;
			}
				
			bobVector = Vector3.Lerp (bobVector, bobPos, bobSmooth * Time.deltaTime);

			// Add input to position
			if (playerController.isEnabled) {
				swayPosition.x -= xInput * swaySpeedX * Time.deltaTime / 10 * speedMultiplier;
				swayPosition.y -= yInput * swaySpeedY * Time.deltaTime / 10 * speedMultiplier;
				swayPosition.z -= zInput * swaySpeedZ * Time.deltaTime / 10 * speedMultiplier;
			}

			// Retrun to original pos overtime
			swayPosition.x = Mathf.Lerp (swayPosition.x, originalViewPos.x + curOffset.x, swayReturnSpeedX * Time.deltaTime * aimMoveMultiplier);
			swayPosition.y = Mathf.Lerp (swayPosition.y, originalViewPos.y + curOffset.y, swayReturnSpeedY * Time.deltaTime * aimMoveMultiplier);
			swayPosition.z = Mathf.Lerp (swayPosition.z, originalViewPos.z + curOffset.z, swayReturnSpeedZ * Time.deltaTime * aimMoveMultiplier);

			// Clamp swayPosition
			swayPosition.x = Mathf.Clamp (swayPosition.x, -swayScaleX, swayScaleX);
			swayPosition.y = Mathf.Clamp (swayPosition.y, -swayScaleY, swayScaleY);
			swayPosition.z = Mathf.Clamp (swayPosition.z, -swayScaleZ, swayScaleZ);

			armHolder.transform.localPosition = swayPosition + bobVector + kickPosVector + weaponOffset;
		}
	}

	void AnimateBob() {
		if (controller.isGrounded && controller.velocity.magnitude > 0 && playerController.isActive) {

			float aimRangeMultiplier = 1f;
			float aimSpeedMultiplier = 1f;

			// Set move multiplier according to if player is sprinting or not
			float moveMultiplier = 1;
			float bobScaleMultiplier = 1;
			if (playerController.curSpeed == playerController.runSpeed) {
				moveMultiplier = 1.4f;
				bobScaleMultiplier = 3f;
			} else {
				moveMultiplier = 1f;
				bobScaleMultiplier = 1f;
			}

			// Smaller bob range when aiming
			if (isChargingSecondaryAction) {
				aimRangeMultiplier = .4f;
				aimSpeedMultiplier = .3f;
			}

			// HORIZONTAL
			//float perc = 1 / (playerController.moveSpeed / controller.velocity.magnitude);
			if (hPositive) {
				curHorizontalRange += (Time.deltaTime * bobSpeed / 2 * aimSpeedMultiplier * playerController.speedMultiplier * moveMultiplier);
				if (curHorizontalRange >= horizontalRange * aimRangeMultiplier) {
					curHorizontalRange = horizontalRange * aimRangeMultiplier;
					hPositive = false;
					AudioManager.instance.PlaySound2D (footStepSounds [UnityEngine.Random.Range (0, footStepSounds.Length)], .1f);
				}
			} else {
				curHorizontalRange -= (Time.deltaTime * bobSpeed / 2 * aimSpeedMultiplier * playerController.speedMultiplier * moveMultiplier);
				if (curHorizontalRange <= -horizontalRange * aimRangeMultiplier) {
					curHorizontalRange = -horizontalRange * aimRangeMultiplier;
					hPositive = true;
					AudioManager.instance.PlaySound2D (footStepSounds [UnityEngine.Random.Range (0, footStepSounds.Length)], .1f);
				}
			}

			// VERTICAL
			if (vPositive) {
				curVerticalRange += (Time.deltaTime * bobSpeed * aimSpeedMultiplier * playerController.speedMultiplier * moveMultiplier);
				if (curVerticalRange >= verticalRange * aimRangeMultiplier) {
					curVerticalRange = verticalRange * aimRangeMultiplier;
					vPositive = false;
				}
			} else {
				curVerticalRange -= (Time.deltaTime * bobSpeed * aimSpeedMultiplier * playerController.speedMultiplier * moveMultiplier);
				if (curVerticalRange <= -verticalRange * aimRangeMultiplier) {
					curVerticalRange = -verticalRange * aimRangeMultiplier;
					vPositive = true;
				}
			}
		
			bobPos.y += curVerticalRange * Time.deltaTime * bobScaleMultiplier;
			bobPos.x += curHorizontalRange * Time.deltaTime * bobScaleMultiplier;
		}
	}
}
