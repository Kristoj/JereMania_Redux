using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
public class Equipment : Item {
	
	public enum FireMode {Auto, Semi}
	[Header("Melee Properties")]
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
	public enum HoldStyle {Hammer, Trovel, Unarmed};
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
		timeBetweenShots = 60 / rpm;
	}

	public virtual void OnPrimaryAction() {
		if (weaponController != null) {
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

	public virtual IEnumerator StartAttack() {
		if (fireMode == FireMode.Semi && !weaponController.mouseLeftReleased) {
			yield break;
		}

		if (Time.time - lastShotTime > timeBetweenShots) {
			playerAnimationController.Attack();
			lastShotTime = Time.time;
			weaponController.isAttacking = true;
			playerStats.StaminaRemove (3f, true);
			// Audio
			if (attackSound != null) {
				StartCoroutine (HitAudioDelay ());
			}

			StartCoroutine (AttackCycle ());
			StartCoroutine (TrackEquipmentVelocity ());

			yield return new WaitForSeconds (hitDelay);
			// Rot
			Vector3 tracerRot = new Vector3 (player.cam.transform.eulerAngles.x, player.cam.transform.eulerAngles.y, player.cam.transform.eulerAngles.z);

			// Raycast
			Ray ray = new Ray (player.cam.transform.position, Quaternion.Euler (tracerRot) * Vector3.forward);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, meleeRange, myHitMask, QueryTriggerInteraction.Collide)) {
				// Take damage
				LivingEntity livingEntity = hit.collider.GetComponent<LivingEntity>();
				if (livingEntity != null) {
					TakeDamage (hit.collider.name, livingEntity.entityGroupIndex, transform.name);
					if (playerStats != null) {
						playerStats.HungerRemove (.09f);
						playerStats.FatiqueRemove (.005f);
					}
					// Add force
					Vector3 meleeForce = equipmentVelocity * impactForce;
					CmdAddImpactForce (meleeForce, hit.point, livingEntity.name, livingEntity.entityGroupIndex);
				}

				// Add impact force
				Entity entity = hit.collider.GetComponent<Entity>();
				if (entity != null && livingEntity == null) {
					Vector3 meleeForce = equipmentVelocity * impactForce;
					CmdAddImpactForce (meleeForce, hit.point, entity.name, entity.entityGroupIndex);
				}

				// Animation
				playerAnimationController.MeleeImpact();

				Quaternion impactRot = Quaternion.identity;
				if (hit.normal != Vector3.zero) {
					impactRot = Quaternion.LookRotation (hit.normal);
				}

				// Play impact audio
				if (entity != null) {
					AudioManager.instance.CmdPlayEntityImpactSound (entity.entitySoundMaterial.ToString(), this.weaponImpactSoundMaterial.ToString(), hit.point, "", 1f);
				} else {
					AudioClip genericClip = SoundLibrary.instance.GetEntityImpactSound ("Generic", weaponImpactSoundMaterial.ToString());
					if (genericClip != null) {
						AudioManager.instance.CmdPlaySound (genericClip.name, hit.point, "", 1);
					}
				}

				// Tell server to spawn tracer for all clients
				CmdOnActionHit (hit.point, transform.name, Quaternion.Euler (tracerRot), impactRot);
			}
		}
	}

	public virtual void Attack() {
		StartCoroutine (StartAttack ());
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
		yield return new WaitForSeconds (.13f);
		// Throw equipment with delay
		if (eventTriggered) {
			weaponController.WipeCurrentEquipmentSlot ();
			weaponController.EquipEquipment(null, true, 6);
		}
		// Enable attacking
		weaponController.isChargingSecondaryAction = false;
		weaponController.canAttack = true;
	}

	IEnumerator ConsumeItem() {
		EquipmentLibrary.instance.ConsumeItem (this.entityName, owner.name);
		AudioManager.instance.CmdPlaySound2D ("UI_Eat1",transform.position, owner.name, 1);
		weaponController.EquipEquipment (null, false, 0);
		yield return new WaitForSeconds (1);
	}

	public override void OnStartInteraction(string masterId) {

		base.OnStartInteraction (masterId);
		if (isAvailable) {
			// Play sound
			if (pickupSound != null) {
				AudioManager.instance.CmdPlaySound (pickupSound.name, transform.position, "", 1);
			}
			PlayerInteraction playerIntera = GameManager.GetLocalPlayer().GetComponent<PlayerInteraction> ();
			playerIntera.StartCoroutine ("PickupItemFollow", entityName);
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
			weaponController.EquipEquipment (this, true, 1);
		}
	}


	IEnumerator AttackCycle() {
		yield return new WaitForSeconds (60 / rpm);
		weaponController.isAttacking = false;
	}

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



	public virtual void TakeDamage(string victimId, int targetGroup, string playerId) {
		
	}

	[Command]
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
	protected void CmdOnActionHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		RpcOnActionHit (hitPoint, id, tracerRot, impactRot);
	}

	[ClientRpc]
	protected void RpcOnActionHit(Vector3 hitPoint, string id, Quaternion tracerRot, Quaternion impactRot) {
		// Spawn impactFX
		if (impactFX != null) {
			Instantiate (impactFX, hitPoint, impactRot);
		}
	}

	IEnumerator HitAudioDelay() {
		yield return new WaitForSeconds (audioDelay);
		AudioManager.instance.CmdPlaySound2D (attackSound.name, transform.position, owner.name, 1);
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
			actionIds [0] = 0;
			actionIds[3] = 2;
			actionIds [4] = 1;
		}

		if (holdStyle == HoldStyle.Unarmed) {
			actionIds [0] = 69;
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


	public void SetOwner (Transform newOwner, string ownerName) {
		owner = newOwner;
		weaponController = owner.GetComponent<GunController> ();
		player = owner.GetComponent<Player> ();
		playerAnimationController = owner.GetComponent<PlayerAnimationController> ();
		playerStats = owner.GetComponent<PlayerStats> ();
		myHitMask = weaponController.hitMask;

		// Set authority
		NetworkIdentity playerId = player.GetComponent<NetworkIdentity> ();
		player.SetAuthority (netId, playerId);
	}

	public virtual void SetHitMask() {
		myHitMask = weaponController.hitMask;
	}
		
	/// <summary>
	/// When this function is called on the server is signals the client who wants to equip an equipment that he can spawn it. After that the entity is destroyed and
	/// unregistered from the gamemanager. Must be called from the client!
	/// </summary>
	public override void CmdDestroyEntity(NetworkInstanceId callerNetId) {
		base.CmdDestroyEntity (callerNetId);
		Debug.Log ("CMD OVERRIDE");
	}
		
	public override void RpcDestroyEntity(NetworkInstanceId callerNetId) {
		base.RpcDestroyEntity(callerNetId);

		if (weaponController.netId == callerNetId) {
			weaponController.canChangeEquipment = true;
		}
		Debug.Log ("RPC OVERRIDE");
	}
}
