using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entity : NetworkBehaviour {

	public string entityName = "Entity";
	[SyncVar]
	public bool isAvailable = true;
	public delegate void DeathDelegate ();
	public DeathDelegate deathEvent;
	public delegate void PickupDelegate();
	public PickupDelegate pickupEvent;
	public EntitySoundMaterial entitySoundMaterial;
	public enum EntitySoundMaterial {Wood, Metal, Rock}

	protected Rigidbody rig;
	[HideInInspector]
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

	public void OnEntityDestroy() {
		if (deathEvent != null) {
			deathEvent ();
		}
	}

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
		entityGroupIndex = i;
	}

	/// <summary>
	/// Destroys the entity from all clients and unregisters it from the gamemanager.
	/// Must be called from the server!
	/// </summary>
	public virtual void DestroyEntity() {
		OnEntityDestroy ();
		GameManager.instance.RemoveEntity (this, entityGroupIndex);
		NetworkServer.Destroy (this.gameObject);
	}


	/// <summary>
	/// Sets the authority for the this entity to the local player. Can be called from server or client.
	/// </summary>
	// Set authority for the player who wants to control this entity
	public void GiveAuthorityToPlayer() {
		GameManager.GetLocalPlayer().SetAuthority (this.netId, GameManager.GetLocalPlayer().GetComponent<NetworkIdentity>());
	}

	/// <summary>
	/// Sets the entity parent. HUOM! Target parent should always be a entity! If not this method will be higly taxing on the CPU!
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
