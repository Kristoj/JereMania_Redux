using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Resource))]

public class ResourceChunk : LivingEntity {

	private Item item;
	public DynamicResourceEntity originalPrefab;

	public override void Start() {
		base.Start ();
		item = GetComponent<Item> ();
	}

	public override void Die() {
		base.Die ();
		rig.isKinematic = false;
		item.isAvailable = true;
	}

	[ClientRpc]
	public override void RpcDie() {
		rig = GetComponent<Rigidbody> ();
		rig.isKinematic = false;
	}

	public void SetStatic() {
		rig = GetComponent<Rigidbody> ();
		item = GetComponent<Item> ();
		item.isAvailable = false;
		rig.isKinematic = true;
	}

	[ClientRpc]
	public void RpcSetStatic() {
		rig = GetComponent<Rigidbody> ();
		item = GetComponent<Item> ();
		item.isAvailable = false;
		rig.isKinematic = true;
	}
		
	public void SetHarvestMode(int chunkId, NetworkInstanceId blockNetId) {
		SetStatic ();
		GetComponent<MeshFilter> ().mesh = originalPrefab.chunkMeshes [chunkId];
		GetComponent<MeshCollider> ().sharedMesh = originalPrefab.chunkMeshes [chunkId];
		transform.parent = WorldManager.instance.spawnedItemHolder;
	}
}
