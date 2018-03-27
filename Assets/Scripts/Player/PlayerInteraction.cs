using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInteraction : NetworkBehaviour {

	[Header("Focus")]
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
	private ChildInteractable targetChildIntera;
	private Player player;
	private PlayerWeaponController weaponController;
	private PlayerAnimationController animationController;
	public bool isPickingUpEquipment = false;

	// Use this for initialization
	void Start () {
		player = GetComponent<Player> ();
		weaponController = GetComponent<PlayerWeaponController> ();
		animationController = GetComponent<PlayerAnimationController> ();
	}
	
	// Update is called once per frame
	void Update () {
		CheckPlayerInput ();
	}

	// CLIENT Interaction Ray
	void ClientUseRay(int buttonId) {
		Ray ray = new Ray (player.cam.transform.position, player.cam.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, focusRange, focusMask, QueryTriggerInteraction.Collide)) {

			// Check if we hit a interactable object
			focusIntera = hit.collider.GetComponent<Interactable> ();
			if (focusIntera != null) {
				targetIntera = focusIntera;

				// Add pickup delay if picking up an item using F
				if (buttonId == 1) {
					if (!isPickingUpEquipment) {
						StartCoroutine (PickupDelay (buttonId));
					}
				} else {
					CmdInteractionConfirm (GetComponent<NetworkIdentity> (), targetIntera.name, targetIntera.entityGroupIndex, buttonId);
				}

				// Animation
				animationController.PickupItem (buttonId);
			} 
			// Check if we hit a child interactable object
			else {
				ChildInteractable childIntera = hit.collider.GetComponent<ChildInteractable>();
				if (childIntera != null) {
					childIntera.OnClientStartInteraction (transform.name);
					CmdChildInteractionConfirm (GetComponent<NetworkIdentity> (), childIntera.parentEntity.name, childIntera.GetType().ToString(), childIntera.name,childIntera.parentEntity.entityGroupIndex, buttonId);
				}
			}
		}
	}
		
	[Command]
	// SERVER interaction confirm
	void CmdInteractionConfirm(NetworkIdentity targetPlayer, string targetEntityGameObjectName, int entityGroup, int buttonId) {
		// Get the target entity tha we interacted with
		Entity interaObject = GameManager.instance.GetEntity(targetEntityGameObjectName, entityGroup);
		if (interaObject == null) {
			interaObject = GameManager.instance.GetLivingEntity (targetEntityGameObjectName,  entityGroup) as Entity;
		}
		// Interact with the object if we got valid reference to it
		if (interaObject != null) {
			targetIntera = interaObject.GetComponent<Interactable> ();
			if (targetIntera != null && targetIntera.isAvailable) {
				// Set authority for the object that we interacted
				if (player == null) {
					player = GetComponent<Player> ();
				}
				player.SetAuthority (targetIntera.netId, targetPlayer);

				// Server interaction
				if (buttonId == 0) {
					targetIntera.OnServerStartInteraction (targetPlayer.name);
				} else if (buttonId == 1) {
					targetIntera.OnServerStartSwap (targetPlayer.name);
				}
				// Rpc the interaction
				RpcInteractionConfirm (targetPlayer, buttonId);
			}
		}
	}

	[ClientRpc]
	// Confirm our child interaction
	void RpcInteractionConfirm(NetworkIdentity targetPlayer, int buttonId) {
		// Check if we're the client who started this interaction
		if (GameManager.GetLocalPlayer().netId == targetPlayer.netId && targetIntera != null) {
			if (buttonId == 0) {
				targetIntera.OnClientStartInteraction (transform.name);
			} else if (buttonId == 1) {
				if (targetIntera as Equipment != null) {
					(targetIntera as Equipment).SetOwner (transform.name);
					targetIntera.OnClientStartSwap (transform.name);
				}
			}
		}
	}

	[Command]
	// Confirm our child interaction
	void CmdChildInteractionConfirm(NetworkIdentity targetPlayer, string parentEntityGameObjectName, string childEntityType,string childEntityName, int entityGroup, int buttonId) {
		// Get the parent entity tha we interacted with
		ParentEntity parentEntity = GameManager.instance.GetEntity(parentEntityGameObjectName, entityGroup) as ParentEntity;
		if (parentEntity == null) {
			return;
		}

		// Get the child entity tha we interacted with
		targetChildIntera = parentEntity.GetChildEntity (childEntityType, childEntityName) as ChildInteractable;
		// Interact with the object if we got valid reference to it
		if (targetChildIntera != null) {
			if (targetChildIntera != null) {
				// Set authority for the object that we interacted
				if (player == null) {
					player = GetComponent<Player> ();
					player.SetAuthority (targetIntera.netId, targetPlayer);
				}
				// Server side interaction
				if (buttonId == 0) {
					targetChildIntera.OnServerStartInteraction (targetPlayer.transform.name);
				} 
				// Server side pickup
				else if (buttonId == 1) {
					targetChildIntera.OnServerStartPickup (targetPlayer.transform.name);
				}
			}
		}
	}

	// Check for player input
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
				CmdOnInteraExit ();
			} else if (targetChildIntera != null) {
				CmdOnChildInteraExit ();
			}
		}
	}

	[Command]
	void CmdOnInteraExit() {
		if (targetIntera != null) {
			targetIntera.OnServerExit (transform.name);
			targetIntera = null;
		}
	}

	[Command]
	void CmdOnChildInteraExit() {
		if (targetChildIntera != null) {
			targetChildIntera.OnServerExit (transform.name);
			targetChildIntera = null;
		}
	}


	// Item that player picked up follows the hand for x amount of time
	IEnumerator PickupItemFollow(string _name) {
		yield return new WaitForSeconds (.14f);
		Equipment eq = Instantiate (ItemDatabase.instance.GetEquipment(_name), weaponController.gunHoldL.position, weaponController.gunHoldL.rotation) as Equipment;
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

	// Add pickup delay when picking up a equipment with 'F'
	IEnumerator PickupDelay(int buttonId) {
		isPickingUpEquipment = true;
		yield return new WaitForSeconds (.18f);
		isPickingUpEquipment = false;
		CmdInteractionConfirm (GetComponent<NetworkIdentity> (), targetIntera.name, targetIntera.entityGroupIndex, buttonId);
	}
}
