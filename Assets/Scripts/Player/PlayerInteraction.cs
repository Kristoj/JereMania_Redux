using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInteraction : NetworkBehaviour {

	[Header("Focus")]
	private Transform cam;
	public float focusRange = 3f;
	[HideInInspector]
	public float curCarryDistance;
	public LayerMask focusMask;
	public LayerMask useMask;

	[Header("Audio")]
	public AudioClip throwSound;

	// Classes
	//[HideInInspector]
	public Interactable focusIntera;
	//[HideInInspector]
	public Interactable targetIntera;
	private Player player;
	private GunController weaponController;
	private PlayerAnimationController animationController;
	private bool isPickingUpEquipment = false;

	// Use this for initialization
	void Start () {
		player = GetComponent<Player> ();
		weaponController = GetComponent<GunController> ();
		animationController = GetComponent<PlayerAnimationController> ();
		cam = player.cam.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) {
			CheckPlayerInput ();
		}
	}

	[Command]
	void CmdInteractEnd (string targetID) {
		
	}

	[Command]
	void CmdOnExit() {
		if (targetIntera != null) {
			targetIntera.OnExit (transform.name);
			targetIntera = null;
		}
	}

	// CLIENT Focus Ray
	void ClientUseRay(int buttonId) {
		Ray ray = new Ray (cam.position, cam.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, focusRange, focusMask, QueryTriggerInteraction.Collide)) {

			focusIntera = hit.collider.GetComponent<Interactable> ();
			if (focusIntera != null) {
				targetIntera = focusIntera;

				// Add pickup delay if picking up an item using F
				if (buttonId == 1) {
					if (!isPickingUpEquipment) {
						StartCoroutine (PickupDelay (buttonId));
					}
				} else {
					CmdUseRay (GetComponent<NetworkIdentity> (), targetIntera.netId, buttonId);
				}

				// Animation
				animationController.PickupItem (buttonId);
			} else {
				ChildInteractable childIntera = hit.collider.GetComponent<ChildInteractable>();
				if (childIntera != null) {
					childIntera.OnClientStartInteraction (transform.name);
				}
			}
		}
	}

	IEnumerator PickupDelay(int buttonId) {
		isPickingUpEquipment = true;
		yield return new WaitForSeconds (.18f);
		isPickingUpEquipment = false;
		CmdUseRay (GetComponent<NetworkIdentity> (), targetIntera.netId, buttonId);
	}
	// SERVER Focus Ray
	[Command]
	void CmdUseRay(NetworkIdentity targetPlayer, NetworkInstanceId interaId, int buttonId) {

		GameObject interaObject = NetworkServer.FindLocalObject (interaId).gameObject;
		if (interaObject != null) {
			targetIntera = interaObject.GetComponent<Interactable> ();
			if (targetIntera != null && targetIntera.isAvailable) {
				SetAuthority (targetPlayer, interaId, buttonId);
			}
		}
		/**

		Ray ray = new Ray (pos, dir);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, focusRange, focusMask, QueryTriggerInteraction.Collide)) {
			focusIntera = hit.collider.GetComponent<Interactable> ();

			if (focusIntera != null && focusIntera.isAvailable) {
				targetIntera = focusIntera;
				// Set authority
				SetAuthority (targetPlayer, interaId, buttonId);
			} else {
				Debug.Log ("Server Ray is NULL");
			}
		}
		**/
	}

	void SetAuthority(NetworkIdentity targetPlayer, NetworkInstanceId interaId, int buttonId) {
		NetworkIdentity targetIdentity = targetIntera.GetComponent<NetworkIdentity>();
		var currentOwner = targetIdentity.clientAuthorityOwner;
		if (currentOwner == targetPlayer.connectionToClient) {
			
		} else {
			if (currentOwner != null) {
				targetIdentity.RemoveClientAuthority (currentOwner);
			}
			targetIdentity.AssignClientAuthority (targetPlayer.connectionToClient);
		}
		// Send message to the client so he can interact with target object
		player = GetComponent<Player> ();
		var msg = new MyMessage();
		msg.playerId = netId;

		if (buttonId == 0) {
			msg.message = "Interact";
		} else {
			msg.message = "Pickup";
		}
		base.connectionToClient.Send(player.myMsgId, msg);
	}

	void CheckPlayerInput() {
		// Interaction
		if (Input.GetKeyDown (KeyCode.E) && !weaponController.isAttacking) {
			ClientUseRay (0);
		}

		// Pickup
		if (Input.GetKeyDown (KeyCode.F) && !weaponController.isAttacking && weaponController.canChangeEquipment && !weaponController.isChargingSecondaryAction) {
			ClientUseRay (1);
		}

		// Drop
		if (Input.GetKeyUp (KeyCode.E)) {
			//CmdInteractEnd (transform.name);
		}

		// Exit
		if (Input.GetKeyDown (KeyCode.Q) && !weaponController.isAttacking) {
			if (targetIntera != null) {
				targetIntera.OnExit (transform.name);
				targetIntera = null;
			}
		}
	}

	IEnumerator PickupItemFollow(string _name) {
		yield return new WaitForSeconds (.14f);
		Equipment eq = Instantiate (EquipmentLibrary.instance.GetEquipment(_name), weaponController.gunHoldL.position, weaponController.gunHoldL.rotation) as Equipment;
		eq.transform.SetParent (weaponController.gunHoldL.transform);
		eq.enabled = false;
		eq.isAvailable = false;
		eq.GetComponent<Rigidbody> ().isKinematic = true;
		eq.GetComponent<Collider> ().enabled = false;
		for (int i = 0; i < eq.transform.childCount; i++) {
			if (eq.transform.GetChild (i).GetComponent<Collider>() != null) {
				eq.transform.GetChild (i).GetComponent<Collider> ().enabled = false;
			}
		}
		yield return new WaitForSeconds (.4f);
		Destroy (eq.gameObject);
	}
}
