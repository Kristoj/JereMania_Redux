using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Unarmed : Weapon {

	[Header ("Lifting")]
	public float liftSpeed = 30f;
	[Range (1, 5)]
	public float maxLiftDistance = 3f;
	public float dropForce = 150f;
	public float distanceChangeSensitivity = 3f;
	public float rotateSensitivity = 3f;

	private float minLiftDistance = 1f;
	private float liftDst;
	private Vector3 targetPos;

	[Header ("Networking")]
	[Tooltip("How many times in a second the target entity will update its velocity for other clients.")]
	public int syncRate = 10;

	// Classes
	public Rigidbody targetRig;
	public Entity targetEntity;

	// Coroutines
	private Coroutine clientOrientationUpdateCoroutine;
	private Coroutine liftCoroutine;

	public override void OnClientEntityHit(string playerName, string entityName, int entityGroup) {
		base.OnClientEntityHit (playerName, entityName, entityGroup);
	}

	public override void OnServerEntityHit(string playerName, string entityName, int entityGroup) {
		// Get reference to the target entity
		targetEntity = GameManager.instance.GetEntity (entityName, entityGroup) as Entity;

		if (targetEntity != null && targetEntity.isAvailable) {
			// Set authority
			GameManager.GetPlayerByName (playerName).SetAuthority (targetEntity.netId, GameManager.GetPlayerByName (playerName).GetComponent<NetworkIdentity> ());
			// Subscride to target entitys on pickup and TODO on use methods
			targetEntity.pickupEvent += CmdYieldEntity;
			// Confrim hit and store target rig
			targetRig = targetEntity.GetComponent<Rigidbody> ();
			targetEntity.isAvailable = false;
			RpcConfirmLift (playerName, entityName, entityGroup);
		}
	}

	[ClientRpc]
	void RpcConfirmLift(string playerName, string entityName, int entityGroup) {
		targetEntity = GameManager.instance.GetEntity (entityName, entityGroup) as Entity;
		if (targetEntity != null) {
			// Store target rigidbody reference
			targetRig = targetEntity.GetComponent<Rigidbody> ();

			// If we're the player who issued this lift, start lifting
			if (GameManager.GetLocalPlayer ().name == playerName) {
				if (liftCoroutine == null) {
					liftCoroutine = StartCoroutine (Lift ());
					StartCoroutine (UpdateVelocity ());
				}
			}
		}
	}

	// Lift loop
	IEnumerator Lift() {
		// If target entity doesn't have rigidbody attached to it don't lift it
		if (targetRig == null) {
			yield break;
		}

		// Set lift distance according to how far the target entity is
		liftDst = Vector3.Distance (player.cam.transform.position, targetEntity.transform.position);
		liftDst = Mathf.Clamp (liftDst, minLiftDistance, maxLiftDistance);

		// Set target rig properties
		targetRig.useGravity = false;

		// Lift target entity while left mouse is down
		while (weaponController.mouseLeftDown && targetEntity != null) {
			// Add scroll input to lift distance
			liftDst += Input.GetAxisRaw ("Mouse ScrollWheel") * distanceChangeSensitivity;
			// Orientate entity
			OrientateEntity ();
			// Throwing
			if (weaponController.mouseRightDown) {
				CmdThrowEntity (player.cam.transform.forward * dropForce, owner.transform.name);
				ThrowEntity (player.cam.transform.forward * dropForce);
				yield break;
			}
			yield return new WaitForFixedUpdate();
		}

		Debug.Log ("We continue");

		// Set target rig properties
		targetRig.useGravity = true;
		targetRig = null;
		targetEntity = null;

		// Drop entity for other clients
		CmdDrop ();
	}

	// Orientate target entity
	void OrientateEntity() {
		// Get mouse input
		targetPos = player.cam.transform.position + (player.cam.transform.forward * liftDst);

		// Add force to lifting direction
		targetRig.AddForce ((targetPos - targetRig.position) * liftSpeed * rig.mass);

		// Zero velocities after applying them to get rid of buoyancy
		targetRig.velocity = Vector3.zero;
		targetRig.angularVelocity = Vector3.zero;

		// Rotation
		if (weaponController.keyRDown) {
			// Get input
			float xInput = Input.GetAxisRaw ("Mouse X");
			float yInput = Input.GetAxisRaw ("Mouse Y");
			playerController.cameraEnabled = false;

			// Rotate target rig
			Vector3 targetEuler = player.cam.transform.TransformDirection (new Vector3 (yInput, -xInput, 0) * rotateSensitivity);
			targetRig.transform.Rotate (targetEuler, Space.World);
		} else {
			playerController.cameraEnabled = true;
		}
	}

	[Command]
	// Signal throw
	void CmdThrowEntity (Vector3 dropDir, string playerName) {
		if (targetEntity != null) {
			targetEntity.isAvailable = true;
			// Unsubscribe from delegates
			targetEntity.deathEvent -= CmdYieldEntity;
			RpcThrowEntity (dropDir, playerName);
		}
	}

	// Signal throw this object
	void RpcThrowEntity(Vector3 dropDir, string playerName) {
		if (targetRig != null && GameManager.GetLocalPlayer().name != playerName) {
			ThrowEntity (dropDir);
		}
	}

	void ThrowEntity(Vector3 dropDir) {
		targetRig.AddForce (dropDir);
		// Set target rig properties
		targetRig.useGravity = true;
		targetRig = null;
		targetEntity = null;
	}

	[Command]
	void CmdDrop() {
		if (targetEntity != null) {
			targetEntity.isAvailable = true;
			// Unsubscribe from delegates
			targetEntity.deathEvent -= CmdYieldEntity;
			RpcDrop ();
		}
	}

	[ClientRpc]
	void RpcDrop() {
		if (targetRig != null) {
			// Set target rig properties
			targetRig.useGravity = true;
			targetRig = null;
			targetEntity = null;
		}
	}

	// While we're lifting a entity, update its velocity for other clients
	IEnumerator UpdateVelocity() {
		float tickRate = 1f / syncRate;

		while (targetRig != null) {
			CmdUpdateVelocity (targetRig.position, targetRig.rotation.eulerAngles);
			yield return new WaitForSeconds (tickRate);
		}
	}

	[Command]
	void CmdUpdateVelocity(Vector3 pos, Vector3 euler) {
		RpcUpdateVelocity (pos, euler);
	}

	[ClientRpc]
	void RpcUpdateVelocity(Vector3 pos, Vector3 euler) {
		if (targetRig != null) {
			targetRig.velocity = Vector3.zero;
			targetRig.angularVelocity = Vector3.zero;
		}

		if (clientOrientationUpdateCoroutine == null) {
			clientOrientationUpdateCoroutine = StartCoroutine(ClientEntityOrientationUpdate (pos, euler));
		}
	}

	// Update target entity rotation for every client
	IEnumerator ClientEntityOrientationUpdate(Vector3 masterPos, Vector3 masterEuler) {

		// Don't sync if target reference is NULL
		if (targetRig == null) {
			yield break;
		}

		// Vars
		Vector3 targetEuler = targetRig.rotation.eulerAngles;
		Vector3 targetPos = targetRig.position;

		// Rotate to target rotation
		while (targetRig != null) {
			targetPos = Vector3.Lerp (targetRig.position, masterPos, 3 * Time.deltaTime);
			// Lerp target euler to the master euler (player who is lifting the object)
			targetEuler.x = Mathf.LerpAngle (targetRig.rotation.eulerAngles.x, masterEuler.x, 3 * Time.deltaTime);
			targetEuler.y = Mathf.LerpAngle (targetRig.rotation.eulerAngles.y, masterEuler.y, 3 * Time.deltaTime);
			targetEuler.z = Mathf.LerpAngle (targetRig.rotation.eulerAngles.z, masterEuler.z, 3 * Time.deltaTime);
			targetRig.transform.eulerAngles = targetEuler;
			yield return null;
		}

	}

	[Command]
	// Called when player is lifting a entity and same entity is picked up or added to inventory
	void CmdYieldEntity() {
		isAvailable = false;

		// Unsubscribe from delegates
		if (targetEntity != null) {
			targetEntity.deathEvent -= CmdYieldEntity;
		}
	}

	// When yielding, set target references to null for every client
	[ClientRpc]
	void RpcYieldEntity() {
		targetEntity = null;
		targetRig = null;
	}
}