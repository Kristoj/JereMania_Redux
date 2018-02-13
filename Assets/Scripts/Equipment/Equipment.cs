using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
public class Equipment : Item {

	[Header ("Equipment Type")]
	public HoldStyle holdStyle;
	public enum HoldStyle {Hammer, Trovel};
	public ActionType primaryAction = ActionType.Attack;
	public ActionType secondaryAction = ActionType.Throw;
	public enum ActionType{Attack, Throw, Block, Consume, Custom};
	public WeaponImpactSoundMaterial weaponImpactSoundMaterial;
	public enum WeaponImpactSoundMaterial {Metal_Slash, Metal_Blunt, Trovel}

	[Header ("Equipment Properties")]
	public Vector3 positionOffset;
	public float speedMultiplier = 1f;
	public bool canRunWith = true;

	[Header("Audio")]
	public AudioClip pickupSound;
	[HideInInspector]
	public LayerMask myHitMask;

	// Classes
	[HideInInspector]
	public Player player;
	protected PlayerAnimationController playerAnimationController;
	protected GunController weaponController;
	protected PlayerStats playerStats;

	public override void Start() {
		base.Start ();
	}

	public virtual void OnPrimaryAction() {
		if (!weaponController.isChargingSecondaryAction) {
			// Attack
			if (primaryAction == ActionType.Attack && playerStats.fatique > 0 && playerStats.stamina > 0) {
				Attack ();
			}
		// Throw
		else if (primaryAction == ActionType.Throw) {
				StartCoroutine (ActionStart (0));
			}
		// Block
		else if (primaryAction == ActionType.Block) {
				StartCoroutine (ActionStart (0));
			}
		// Consume
		else if (primaryAction == ActionType.Consume) {
				StartCoroutine (ActionStart (0));
			}
		// Custom
		else if (primaryAction == ActionType.Custom) {

			}
		}
	}

	// SECONDARY Action
	public virtual void OnSecondaryAction() {
		// Attack
		if (secondaryAction == ActionType.Attack) {
			Attack ();
		}
		// Throw
		else if (secondaryAction == ActionType.Throw) {
			StartCoroutine (ActionStart(1));
		}
		// Block
		else if (secondaryAction == ActionType.Block) {
			StartCoroutine (ActionStart(1));
		}
		// Consume
		else if (secondaryAction == ActionType.Consume) {
			StartCoroutine (ActionStart(1));
		}
		// Custom
		else if (secondaryAction == ActionType.Custom) {

		}
	}

	public virtual void Attack() {

	}
		
	IEnumerator ActionStart(int buttonId) {
		weaponController.animController.ActionStart (buttonId);

		bool eventTriggered = false;
		bool pass = false;
		weaponController.isChargingSecondaryAction = true;
		float t = .3f;

		while (!eventTriggered && !pass) {

			// PRIMARY ACTION ---

			// TRIGGER if left mouse is down and x amount of time has passed
			if (weaponController.mouseLeftDown && buttonId == 0 && t <= 0) {
				if (primaryAction == ActionType.Consume || secondaryAction == ActionType.Consume) {
					eventTriggered = true;
					StartCoroutine (ConsumeItem ());
				}
			}

			if (buttonId == 0 && !weaponController.mouseLeftDown) {
				pass = true;
			}

			// PRIMARY ACTION ---

			// SECONDARY ACTION ---
			if (!weaponController.mouseRightDown && buttonId == 1) {
				eventTriggered = false;
				pass = true;
			}
			// TRIGGER if left mouse is down and trigger button is right click
			if (weaponController.mouseLeftDown && buttonId == 1) {
				// Drop this equipment

				eventTriggered = true;
				weaponController.canChangeEquipment = true;
			}
			// SECONDARY ACTION ---

			if (eventTriggered) {
				weaponController.animController.ActionEvent ();
			}

			t -= Time.deltaTime;
			yield return null;
		}
		if (!eventTriggered) {
			weaponController.animController.ActionEnd ();
		}
		// Enable attacking
		yield return new WaitForSeconds (.15f);
		if (eventTriggered) {
			weaponController.EquipEquipment(weaponController.weapon01, 6);
		}
		weaponController.isChargingSecondaryAction = false;
		weaponController.canAttack = true;
	}

	IEnumerator ConsumeItem() {
		EquipmentLibrary.instance.ConsumeItem (this.objectName, owner.name);
		AudioManager.instance.CmdPlaySound2D ("UI_Eat1",transform.position, owner.name, 1);
		weaponController.DestroyCurrentEquipment ();
		yield return new WaitForSeconds (1);
	}

	public override void OnStartInteraction(string masterId) {

		base.OnStartInteraction (masterId);
		if (isAvailable) {
			// Play sound
			if (pickupSound != null) {
				AudioManager.instance.CmdPlaySound (pickupSound.name, transform.position, "", 1);
			}
			PlayerInteraction playerIntera = GameManager.instance.localPlayer.GetComponent<PlayerInteraction> ();
			playerIntera.StartCoroutine ("PickupItemFollow", objectName);
		}
	}

	public override void OnStartPickup(string masterId) {
		if (isAvailable) {
			base.OnStartPickup (masterId);
			base.CmdOnPickup ();

			// Play sound
			if (pickupSound != null) {
				AudioManager.instance.CmdPlaySound (pickupSound.name, transform.position, "", 1);
			}
			weaponController.EquipEquipment (this, 1);
		}
	}

	public int[] GetAnimationIds() {
		int[] actionIds = new int[5];
		/////////
		/// 
		/// 
		// ActionID - 0 - = Hold Animation
		// ActionID - 1 - = Primary Action
		// ActionID - 2 - = Secondary Action
		// ActionID - 3 - = Attack animation Start Index
		// ActionID - 4 - = Attack animation count


		// Hold id
		// Attack
		if (holdStyle == HoldStyle.Hammer) {
			actionIds [0] = 0;
			actionIds[3] = 0;
			actionIds [4] = 2;
		}

		if (holdStyle == HoldStyle.Trovel) {
			actionIds[3] = 2;
			actionIds [4] = 1;
		}

		// Primary action type id
		// Throw
		else if (primaryAction == ActionType.Throw) {
			actionIds [1] = 0;
		}
		// Block
		else if (primaryAction == ActionType.Block) {
			actionIds [1] = 1;
		}

		// Consume
		else if (primaryAction == ActionType.Consume) {
			actionIds [1] = 2;
		}

		// Custom
		else if (primaryAction == ActionType.Custom) {
			actionIds [1] = 99;
		}

		// Secondary action type id
		// Throw
		if (secondaryAction == ActionType.Throw) {
			actionIds [2] = 0;
		}
		// Block
		else if (secondaryAction == ActionType.Block) {
			actionIds [2] = 1;
		}

		// Consume
		else if (secondaryAction == ActionType.Consume) {
			actionIds [2] = 2;
		}

		// Custom
		else if (secondaryAction == ActionType.Custom) {
			actionIds [2] = 99;
		}
		return actionIds;
	}

	public void SetOwner (Transform newOwner, string ownerId) {
		owner = newOwner;
		weaponController = owner.GetComponent<GunController> ();
		player = owner.GetComponent<Player> ();
		playerAnimationController = owner.GetComponent<PlayerAnimationController> ();
		playerStats = owner.GetComponent<PlayerStats> ();

		// Parent gun
		transform.parent = weaponController.gunHoldR.transform;
		transform.position = weaponController.gunHoldR.transform.position;

		// Set authority
		NetworkIdentity playerId = player.GetComponent<NetworkIdentity> ();
		player.SetAuthority (netId, playerId);
	}

	public virtual void SetHitMask() {
		myHitMask = weaponController.hitMask;
	}
}
