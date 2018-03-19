using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Resource))]

public class ResourceChunk : LivingEntity {

	private Item item;
	public DynamicResourceEntity originalPrefab;

	void Awake() {
		dead = true;
	}

	public override void Start() {
		base.Start ();
		item = GetComponent<Item> ();
	}

	public override void Die(string sourcePlayer) {
		base.Die (sourcePlayer);
		rig.isKinematic = false;
		item.isAvailable = true;
	}
		
	public override void OnClientDie() {
		base.OnClientDie ();
		rig = GetComponent<Rigidbody> ();
		rig.isKinematic = false;
	}

	void SetStatic() {
		rig = GetComponent<Rigidbody> ();
		item = GetComponent<Item> ();
		item.isAvailable = false;
		rig.isKinematic = true;
		dead = false;
	}
		
	public void SetHarvestMode(int chunkId, NetworkInstanceId blockNetId) {
		SetStatic ();
		GetComponent<MeshFilter> ().mesh = originalPrefab.chunkMeshes [chunkId];
		GetComponent<MeshCollider> ().sharedMesh = originalPrefab.chunkMeshes [chunkId];
		transform.parent = WorldManager.instance.spawnedItemHolder;
	}
}
