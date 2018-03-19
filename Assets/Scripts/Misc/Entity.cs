using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entity : NetworkBehaviour {

	public string entityName = "Entity";
	[SyncVar]
	public bool isAvailable = true;
	public delegate void DeathDelegate (string targetPlayer);
	public event DeathDelegate deathEvent;
	public delegate void PickupDelegate(string targetPlayer);
	public event PickupDelegate pickupEvent;
	public EntitySoundMaterial entitySoundMaterial;
	public enum EntitySoundMaterial {Wood, Metal, Rock}

	protected Rigidbody rig;
	[SyncVar]
	public int entityGroupIndex;

	public virtual void Start() {
		rig = GetComponent<Rigidbody> ();
	}

	public override void OnStartClient() {
		base.OnStartClient ();

		if (GetComponent<LivingEntity> () == null) {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			Entity entity = GetComponent<Entity> ();
			GameManager.instance.RegisterEntity (myID, entity, entityName);

		} else {
			string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
			LivingEntity entity = GetComponent<LivingEntity> ();
			GameManager.instance.RegisterLivingEntity (myID, entity, entityName);
		}
	}
		
	public virtual void OnEntityHit(string playerName, string sourceEquipmentName) {
		
	}

	// ------------------------------------------ EVENTS START ------------------------------------------------------------ \\
	public void OnEntityDestroy(string targetPlayer) {
		if (deathEvent != null) {
			deathEvent (targetPlayer);
			deathEvent = null;
		}
	}

	public void OnInteractablePickup(string targetPlayer) {
		if (pickupEvent != null) {
			pickupEvent (targetPlayer);
			pickupEvent = null;
		}
	}
	// ------------------------------------------ EVENTS END ------------------------------------------------------------ \\
	public void AddImpactForce(Vector3 impactForce, Vector3 impactPos) {
		RpcAddImpactForce (impactForce, impactPos);
	}

	[ClientRpc]
	void RpcAddImpactForce(Vector3 impactForce, Vector3 impactPos) {
		// Add force
		if (rig == null) {
			rig = GetComponent<Rigidbody> ();
		}

		if (rig != null) {
			rig.AddForceAtPosition (impactForce, impactPos, ForceMode.Impulse);
		}
	}
		
	public void SetGroupId (int i) {
		Entity[] eList = GetComponents<Entity> ();
		foreach (Entity e in eList) {
			e.entityGroupIndex = i;
		}
	}



	/// <summary>
	/// Destroys the entity from all clients and unregisters it from the gamemanager.
	/// Must be called from the server!
	/// </summary>
	/// <param name="sourcePlayer">Player who called this function.</param>
	public virtual void DestroyEntity(string sourcePlayer) {
		// Call OnEntityDestroy event and disable this entity immediatly
		OnEntityDestroy (sourcePlayer);
		RpcDisableEntity ();
		// Wait for x amount of seconds before completely destroying the entity
		if (gameObject.activeSelf) {
			StartCoroutine (DestroyEntityDelay ());
		} 
		// If this gameobject is not active destroy it immediately
		else {
			GameManager.instance.RemoveEntity (this, entityGroupIndex);
			NetworkServer.Destroy (this.gameObject);
		}
	}

	IEnumerator DestroyEntityDelay() {
		yield return new WaitForSeconds (1f);
		GameManager.instance.RemoveEntity (this, entityGroupIndex);
		NetworkServer.Destroy (this.gameObject);
	}

	public void DisableEntity() {
		RpcDisableEntity ();
	}

	public void EnableEntity(Vector3 wakePos = default(Vector3)) {
		RpcEnableEntity (wakePos);
	}

	[ClientRpc]
	void RpcEnableEntity(Vector3 wakePos) {
		if (wakePos != Vector3.zero) {
			transform.position = wakePos;
		}
		gameObject.SetActive (true);
	}

	[ClientRpc]
	void RpcDisableEntity() {
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Sets the authority for the this entity to the local player. Can be called from server or client.
	/// </summary>
	// Set authority for the player who wants to control this entity
	public void GiveAuthorityToPlayer() {
		GameManager.GetLocalPlayer().SetAuthority (this.netId, GameManager.GetLocalPlayer().GetComponent<NetworkIdentity>());
	}

	/// <summary>
	/// Sets the entity parent. HUOM! Target parent should always be a entity! Otherwise this method will be higly taxing on the CPU!
	/// </summary>
	/// <param name="newParentName">New parent name.</param>
	/// <param name="parentGroup">Parents entityGroupIndex. Can be referenced from parents entity class</param>
	public void SetEntityParent(string newParentName, int parentGroup) {
		RpcSetEntityParent (newParentName, parentGroup);
	}

	[ClientRpc]
	void RpcSetEntityParent(string newParentName, int parentGroup) {
		// Get reference to the parent
		Entity newParent = GameManager.instance.GetEntity (newParentName, parentGroup);
		// If we failed to find parent entity, try finding gameobject in the game world that has its name.. This is highly unoptimized and only should be used as a failsafe
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

	/// <summary>
	/// Sets the entity parent to child entity. HUOM! Target parent should always be a entity! Otherwise this method will be higly taxing on the CPU!
	/// </summary>
	/// <param name="parentEntityName">Parent entity name.</param>
	/// <param name="parentEntityGroup">Parent entity group.</param>
	/// <param name="childEntityName">Child entity name.</param>
	public void SetEntityParentToChildEntity(string parentEntityName, int parentEntityGroup, string childEntityName) {
		RpcSetEntityParentToChildEntity (parentEntityName, parentEntityGroup, childEntityName);
	}

	[ClientRpc]
	void RpcSetEntityParentToChildEntity(string parentEntityName, int parentEntityGroup, string childEntityName) {
		// Get reference to the parent
		ParentEntity parentEntity = GameManager.instance.GetEntity (parentEntityName, parentEntityGroup) as ParentEntity;

		// Get reference to the child entity
		if (parentEntity != null) {
			ChildEntity childEntity = parentEntity.GetChildEntityByName (childEntityName);
			// If we failed to find parent entity, try finding gameobject in the game world that has its name.. This is highly unoptimized and only should be used as a failsafe
			if (parentEntityName == "" && childEntity == null) {
				GameObject  go = GameObject.Find(parentEntityName);
				if (go != null) {
					childEntity = go.GetComponent<ChildEntity>();
				}
			}

			// Parent this object to the parent if it's valid
			if (childEntity != null) {
				transform.SetParent (childEntity.transform);
			}
		}
	}
}
