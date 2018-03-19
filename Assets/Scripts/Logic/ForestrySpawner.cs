using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ForestrySpawner : NetworkBehaviour {

	public Entity[] forestryList;
	public int maxForestryCount = 50;
	public int curForestryCount = 0;
	public float spawnInterval = 60;
	public float xRange = 200f;
	public float zRange = 200f;
	public bool canGrown = true;
	public LayerMask whatToHit;

	// Use this for initialization
	void Start () {
		if (isServer) {
			for (int i = 0; i < maxForestryCount; i++) {
				//SpawnForestry ();
			}
		}
		StartCoroutine (GrowForestry ());

	}
	IEnumerator GrowForestry() {
		yield return new WaitForSeconds (1);
		while (canGrown) {
			yield return new WaitForSeconds (spawnInterval);

			if (curForestryCount < maxForestryCount) {
				if (forestryList.Length > 0) {
					SpawnForestry ();
				}
			}
		}
	}
		

	void SpawnForestry() {
		Ray ray = new Ray (transform.position + (transform.up * 25) + new Vector3 (Random.Range (-xRange, xRange), 0, Random.Range (-zRange, zRange)), -Vector3.up);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 500, whatToHit)) {
			Vector3 spawnPos = hit.point;
			Entity clone = Instantiate (forestryList [Random.Range(0, forestryList.Length)], spawnPos, Quaternion.identity, transform) as Entity;
			NetworkServer.Spawn (clone.gameObject);
			clone.deathEvent += RemoveForestry;
			curForestryCount++;
			StartCoroutine (DelaySetup (clone.name, clone.entityGroupIndex, spawnPos));
			RpcSetForestryStatic (clone.name, clone.entityGroupIndex);
		}
	}

	[ClientRpc]
	void RpcSetForestryStatic(string entityName, int entityGroup) {
		Entity e = GameManager.instance.GetEntity (entityName, entityGroup);
		if (e != null) {
			e.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	IEnumerator DelaySetup(string cloneName, int cloneGroup , Vector3 spawnPos) {
		yield return new WaitForSeconds (.05f);
		Entity e = GameManager.instance.GetEntity (cloneName, cloneGroup);
		if (e != null) {
			DynamicResourceEntity dynamicResourceClone = e.GetComponent<DynamicResourceEntity> ();
			if (dynamicResourceClone) {
				dynamicResourceClone.SetupChunks (spawnPos);
			}
		}

	}

	public void RemoveForestry(string targetPlayer) {
		curForestryCount--;
	}
}
