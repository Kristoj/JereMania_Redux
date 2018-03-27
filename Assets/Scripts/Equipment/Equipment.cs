using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
public class Equipment : Item {
	
	public enum FireMode {Auto, Semi}
	[Header("Equipment Stats")]
	public FireMode fireMode;
	public float rpm = 100;
	protected float lastShotTime;
	protected float timeBetweenShots;
	public float meleeRange = 3f;
	public float hitDelay = .35f;
	public float audioDelay = .15f;
	public float impactForce = 5f;
	private Vector3 equipmentVelocity;

	[Header ("Equipment Type")]
	public HoldStyle holdStyle;
	public enum HoldStyle {Hammer, Trovel, Unarmed, Spear};
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
	public AudioClip attackSound;
	[Header("FX")]
	public ImpactFX impactFX;
	//[HideInInspector]
	public LayerMask myHitMask;

	// Classes
	[HideInInspector]
	public Player player;


	public override void Start() {
		base.Start ();
		timeBetweenShots = 60 / rpm;
	}

	//  ----- INPUT ----- \\
	#region |---Action Input---|
	public virtual void OnPrimaryAction() {
		if (player.weaponController != null) {
			if (!player.weaponController.isChargingSecondaryAction) {
				// Attack
				if (primaryAction == ActionType.Attack && player.playerStats.fatique > 0 && player.playerStats.stamina > 0) {
					Attack ();
				}
				// Throw
				else if (primaryAction == ActionType.Throw) {
					StartCoroutine (ActionCycle (0));
				}
				// Block
				else if (primaryAction == ActionType.Block) {
					StartCoroutine (ActionCycle (0));
				}
				// Consume
				else if (primaryAction == ActionType.Consume) {
					StartCoroutine (ActionCycle (0));
				}
				// Custom
				else if (primaryAction == ActionType.Custom) {

				}
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
			StartCoroutine (ActionCycle(1));
		}
		// Block
		else if (secondaryAction == ActionType.Block) {
			StartCoroutine (ActionCycle(1));
		}
		// Consume
		else if (secondaryAction == ActionType.Consume) {
			StartCoroutine (ActionCycle(1));
		}
		// Custom
		else if (secondaryAction == ActionType.Custom) {
			player.weaponController.isChargingSecondaryAction = false;
			player.weaponController.isAttacking = false;
			player.weaponController.canAttack = true;
		}
	}
	#endregion

	//  ----- Attack Cycle ----- \\
	public virtual IEnumerator AttackCycle() {
		if (fireMode == FireMode.Semi && !player.weaponController.mouseLeftReleased) {
			yield break;
		}

		if (Time.time - lastShotTime > timeBetweenShots) {
			// Set vars
			lastShotTime = Time.time;
			player.weaponController.isAttacking = true;

			// Effects
			player.animationController.Attack();
			if (attackSound != null) {
				StartCoroutine (HitAudioDelay ());
			}

			// Player Conditions
			player.playerStats.StaminaRemove (3f, true);

			// Misc
			StartCoroutine (TrackEquipmentVelocity ());
			yield return new WaitForSeconds (hitDelay);

			// Raycast
			Ray ray = new Ray (player.cam.transform.position, player.cam.transform.forward);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, meleeRange, myHitMask, QueryTriggerInteraction.Collide)) {

				// Get Entity
				Entity entity = hit.collider.GetComponent<Entity>();
				if (entity != null) {
					// Add impact force and tell target entity that we hit it
					Vector3 meleeForce = equipmentVelocity * impactForce;
					CmdAddImpactForce (meleeForce, hit.point, entity.name, entity.entityGroupIndex);
					OnClientEntityHit (entity.name, entity.entityGroupIndex, owner.name);

					// Player conditions
					if (player.playerStats != null) {
						player.playerStats.HungerRemove (.09f);
						player.playerStats.FatiqueRemove (.005f);
					}
				} 

				// Get ChildEntity
				else {
					ChildEntity childEntity = hit.collider.GetComponent<ChildEntity>();
					if (childEntity != null) {
						CmdOnChildEntityHit (owner.name, childEntity.parentEntity.name, childEntity.GetType().ToString(), childEntity.name, childEntity.parentEntity.entityGroupIndex);
					}
				}

				// -------------------------------------- EFFECTS  START ------------------------------------------------ \\
				// Play impact audio
				if (entity != null) {
					AudioManager.instance.CmdPlayEntityImpactSound (entity.entitySoundMaterial.ToString(), this.weaponImpactSoundMaterial.ToString(), hit.point, "", 1f);
				} else {
					AudioClip genericClip = SoundLibrary.instance.GetEntityImpactSound ("Generic", weaponImpactSoundMaterial.ToString());
					if (genericClip != null) {
						AudioManager.instance.CmdPlaySound (genericClip.name, hit.point, "", 1);
					}
				}

				// Animation
				player.animationController.MeleeImpact();

				// Impact rotation
				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}

				// Tell server to spawn ImpactFX for all clients
				Vector3 tracerRot = new Vector3 (player.cam.transform.eulerAngles.x, player.cam.transform.eulerAngles.y, player.cam.transform.eulerAngles.z);
				CmdImpactFX (hit.point, transform.name, Quaternion.Euler (tracerRot), impactRot);
				// -------------------------------------- EFFECTS  END ------------------------------------------------ \\
			}

			// Wait for attack cycle to end
			yield return new WaitForSeconds (60 / rpm - hitDelay);
			player.weaponController.isAttacking = false;
		}
	}

	// Starts the attack cycle...
	public virtual void Attack() {
		StartCoroutine (AttackCycle ());
	}

	//  ----- Action Cycle ----- \\
	IEnumerator ActionCycle(int buttonId) {
		// Set vars and start action animation
		player.weaponController.animController.ActionStart (buttonId);
		player.weaponController.isChargingSecondaryAction = true;
		bool eventTriggered = false;
		bool pass = false;
		float t = .3f;

		while (!eventTriggered && !pass) {

			// --- PRIMARY ACTION START --- \\

			// TRIGGER if left mouse is down and x amount of time has passed
			if (player.weaponController.mouseLeftDown && buttonId == 0 && t <= 0) {
				if (primaryAction == ActionType.Consume || secondaryAction == ActionType.Consume) {
					eventTriggered = true;
					StartCoroutine (ConsumeItem ());
				}
			}

			if (buttonId == 0 && !player.weaponController.mouseLeftDown) {
				pass = true;
			}

			// --- PRIMARY ACTION END --- \\

			// SECONDARY ACTION ---
			if (!player.weaponController.mouseRightDown && buttonId == 1) {
				eventTriggered = false;
				pass = true;
			}
			// TRIGGER if left mouse is down and trigger button is right click
			if (player.weaponController.mouseLeftDown && buttonId == 1) {
				// Drop this equipment

				eventTriggered = true;
				player.weaponController.canChangeEquipment = true;
			}
			// SECONDARY ACTION ---

			if (eventTriggered) {
				player.weaponController.animController.ActionEvent ();
			}

			t -= Time.deltaTime;
			yield return null;
		}
		if (!eventTriggered) {
			player.weaponController.animController.ActionEnd ();
		}

		// If we are the client wait x amount of time according to the last ping before continuing
		float waitTime = .12f;
		if (!isServer) {
			int ping = GameManager.GetLocalPlayerAveragePing() / 1000;

			if (ping < waitTime) {
				float clientWaitTime = waitTime - ping;

				while (clientWaitTime > 0) {
					clientWaitTime -= Time.deltaTime;
					yield return null;
				}
			}
		} 
		// If we're the server wait for specific amount of time before continuing
		else {
			yield return new WaitForSeconds (waitTime);
		}
		// Throw equipment with delay
		if (eventTriggered) {
			player.weaponController.EquipEquipment("", entityGroupIndex, true, 6);
		}
		// Enable attacking
		player.weaponController.isChargingSecondaryAction = false;
		player.weaponController.canAttack = true;
	}

	IEnumerator ConsumeItem() {
		ItemDatabase.instance.ConsumeItem (this.entityName, owner.name);
		AudioManager.instance.CmdPlaySound2D ("UI_Eat1",transform.position, owner.name, 1);
		player.weaponController.EquipEquipment ("", 0, false, 0);
		yield return new WaitForSeconds (1);
	}

	#region INTERACTIONS
	// Client interaction
	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);
		if (isAvailable) {
			// Play pickup sound
			if (pickupSound != null) {
				AudioManager.instance.CmdPlaySound (pickupSound.name, transform.position, "", 1);
			}
			PlayerInteraction playerIntera = GameManager.GetLocalPlayer().GetComponent<PlayerInteraction> ();
			playerIntera.StartCoroutine ("PickupItemFollow", entityName);
		}
	}

	// Client swap
	public override void OnClientStartSwap(string masterId) {
		if (isAvailable) {
			base.OnClientStartSwap (masterId);

			// Play sound
			if (pickupSound != null) {
				AudioManager.instance.CmdPlaySound (pickupSound.name, transform.position, "", 1);
			}
		}
	}

	// Server pickup
	public override void OnServerStartSwap(string masterId) {
		if (isAvailable) {
			base.OnServerStartSwap (masterId);
			GameManager.GetPlayerByName(masterId).GetComponent<PlayerWeaponController>().EquipEquipment (transform.name, entityGroupIndex, true, 1);
		}
	}
	#endregion

	// Called on the client when player hits a entity
	public virtual void OnClientEntityHit(string victimName, int victimGroup, string sourcePlayer) {
		CmdSignalOnEntityHit (victimName, victimGroup, sourcePlayer);
	}

	// Signals on entity hit from a client to the server
	[Command]
	void CmdSignalOnEntityHit (string victimName, int victimGroup, string sourcePlayer) {
		OnServerEntityHit (victimName, victimGroup, sourcePlayer);
	}

	// Called on the server when player hits a entity
	public virtual void OnServerEntityHit(string victimName, int victimGroup, string sourcePlayer) {
		Entity targetEntity = GameManager.instance.GetEntity (victimName, victimGroup);
		if (targetEntity != null) {
			targetEntity.OnEntityHit (sourcePlayer, this.entityName);
		}
	}

	[Command]
	void CmdOnChildEntityHit(string playerName, string parentName, string childType, string childName, int entityGroup) {
		// Get the target living entity that we hit
		ParentEntity parentEntity = GameManager.instance.GetEntity(parentName, entityGroup) as ParentEntity;
		if (parentEntity == null) {
			return;
		}
		// Interact with the object if we got valid reference to it
		if (parentEntity != null) {
			ChildEntity childEntity = parentEntity.GetChildEntity (childType, childName) as ChildEntity;
			if (childEntity != null) {
				// Set authority for the object that we hit
				if (player == null) {
					player = owner.GetComponent<Player> ();
					player.SetAuthority (parentEntity.netId, GameManager.GetPlayerByName(playerName).GetComponent<NetworkIdentity>());
				}
				// Rpc the interaction
				childEntity.OnChildEntityHit(playerName, transform.name);
			}
		}
	}

	[Command]
	// Adds impact force when player hits a entity
	void CmdAddImpactForce(Vector3 meleeForce, Vector3 forcePoint, string targetName, int targetGroup) {
		LivingEntity targetLivingEntity = GameManager.instance.GetLivingEntity (targetName, targetGroup);
		if (targetLivingEntity != null) {
			targetLivingEntity.AddImpactForce (meleeForce, forcePoint);
		} else {
			Entity targetEntity = GameManager.instance.GetEntity (targetName, targetGroup);
			if (targetEntity != null) {
				targetEntity.AddImpactForce (meleeForce, forcePoint);
			}
		}
	}

	[Command]
	// Called when player hits something with PrimaryAction (Left click ability with equipment)
	protected void CmdImpactFX(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		RpcImpactFX (hitPoint, id, tracerRot, impactRot);
	}

	[ClientRpc]
	// Spawn ImpactFX
	protected void RpcImpactFX(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		if (impactFX != null) {
			Instantiate (impactFX, hitPoint, impactRot);
		}
	}

	// Hit audio delay
	IEnumerator HitAudioDelay() {
		yield return new WaitForSeconds (audioDelay);
		AudioManager.instance.CmdPlaySound2D (attackSound.name, transform.position, owner.name, 1);
	}

	// Tracks equipments velocity while player is attacking with his equipment, and when he hits a entity add impact force to it...
	IEnumerator TrackEquipmentVelocity() {
		Vector3 equipmentA = transform.position;
		Vector3 playerA = owner.transform.position;
		yield return new WaitForSeconds (.03f);
		Vector3 equipmentB = transform.position;
		Vector3 playerB = owner.transform.position;
		equipmentVelocity = (equipmentA-equipmentB);
		equipmentVelocity = equipmentVelocity.normalized;
		equipmentVelocity -= (playerA - playerB).normalized;
	}

	// Stores all equipment action animation ids
	public int[] GetAnimationIds() {
		int[] actionIds = new int[10];

		// // // // UPDATE ANIMATION IDs FOR THE ANIMATOR \\ \\ \\ \\
		// ActionID 0 = Hold Animation
		// 	   0 = Hammer | 2 = Spear | 69 = Unarmed
		// ActionID 1 = Primary Action
		// ActionID 2 = Secondary Action
		// ActionID 3 = Attack animation Start Index
		//	   0 = Hammer_Attack_01 | 1 = Hammer_Attack_02 | 2 = Trovel_Attack_01 | 3 = Spear_Attack_01
		// ActionID 4 = Attack animation count


		// Hold id
		// Attack
		if (holdStyle == HoldStyle.Hammer) {
			actionIds [0] = 0;
			actionIds[3] = 0;
			actionIds [4] = 2;
		}

		if (holdStyle == HoldStyle.Trovel) {
			actionIds [0] = 0;
			actionIds[3] = 2;
			actionIds [4] = 1;
		}

		if (holdStyle == HoldStyle.Unarmed) {
			actionIds [0] = 69;
			actionIds[3] = 2;
			actionIds [4] = 1;
		}

		if (holdStyle == HoldStyle.Spear) {
			// Hold id
			actionIds [0] = 2;
			// Attack animation start index
			actionIds[3] = 3;
			// Attack animation length
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

	[ClientRpc]
	public void RpcEquipmentHandshake(string callerName) {
		if (player.weaponController != null) {
			if (GameManager.GetLocalPlayer().name == callerName) {
				player.weaponController.canSpawnNewEquipment = true;
			}
		}
	}

	[ClientRpc]
	public void RpcMoveCurrentEquipment(Vector3 movePos, Vector3 moveRot, string newParentName, int parentGroup) {
		// Orientate this object
		transform.position = movePos;
		transform.eulerAngles = moveRot;

		// Get parent reference
		Entity newParent = GameManager.instance.GetEntity (newParentName, parentGroup);
		if (newParentName == "" && newParent == null) {
			GameObject  go = GameObject.Find(newParentName);
			if (go != null) {
				newParent = go.GetComponent<Entity>();
			}
		}

		// Parent this object to the parent if it's valid
		if (newParent != null) {
			transform.SetParent (newParent.transform);
		}
	}

	[ClientRpc]
	public void RpcDiscardEquipment(string newParentName, int parentGroup) {
		// Get parent reference
		Entity newParent = GameManager.instance.GetEntity (newParentName, parentGroup);
		if (newParentName == "" && newParent == null) {
			GameObject  go = GameObject.Find(newParentName);
			if (go != null) {
				newParent = go.GetComponent<Entity>();
			}
		}

		// Parent this object to the parent if it's valid
		if (newParent != null) {
			transform.SetParent (newParent.transform);
		}
	}

	// Set the owner of this equipment
	public void SetOwner (string ownerName) {
		Player newOwner = GameManager.GetPlayerByName (ownerName);
		if (newOwner != null) {

			owner = newOwner.transform;
			player = owner.GetComponent<Player> ();

			// Set authority
			NetworkIdentity playerId = player.GetComponent<NetworkIdentity> ();
			player.SetAuthority (netId, playerId);
		} else {

		}
	}

	// Sets this equipment control mode to player controlled or control free on the server
	public void SetEquipmentPlayerControlled(bool isPlayerController, string ownerName) {
		// Player controlled
		if (isPlayerController) {
			isAvailable = false;

			// Set new owner
			SetOwner(ownerName);
		}
		// Control free
		else {
			isAvailable = true;
			if (player != null && player.weaponController != null) {
				player.weaponController.currentEquipment = null;
				owner = null;
			}
		}
		RpcSetEquipmentPlayerControlled (isPlayerController, ownerName);
	}

	[ClientRpc]
	// Sets this equipment control mode to player controlled or control free on the client
	void RpcSetEquipmentPlayerControlled(bool isPlayerController, string ownerName) {
		// Player controlled
		if (isPlayerController) {
			// Layers
			gameObject.layer = LayerMask.NameToLayer ("ViewModel");
			if (transform.childCount > 0) {
				for (int i = 0; i < transform.childCount; i++) {
					transform.GetChild (i).gameObject.layer = LayerMask.NameToLayer ("ViewModel");
				}
			}

			// Enable this class for the owner player and disable it for other clients
			if (GameManager.GetLocalPlayer ().name == ownerName) {
				this.enabled = true;
			} else {
				this.enabled = false;
			}

			// Set active
			gameObject.SetActive (true);
		}
		// Control free
		else {
			gameObject.layer = LayerMask.NameToLayer ("Default");
			if (transform.childCount > 0) {
				for (int i = 0; i < transform.childCount; i++) {
					transform.GetChild (i).gameObject.layer = LayerMask.NameToLayer ("Default");
				}
			}
			owner = null;
		}
	}

	public virtual void SetHitMask() {
		myHitMask = player.weaponController.hitMask;
	}
}