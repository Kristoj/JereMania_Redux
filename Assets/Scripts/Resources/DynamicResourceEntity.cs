using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DynamicResourceEntity : Resource {
	
	public Transform chunkPrefab;
	public Mesh[] chunkMeshes;
	private int chunkCount;


	public void SetupChunks(Vector3 setupPos) {
		// Spawn static resource entity
		Vector3 blockPos = setupPos;
		blockPos.y += GetComponent<Collider> ().transform.localScale.y / 2 + .5f;
		chunkCount = chunkMeshes.Length;
		for (int i = 0; i < chunkMeshes.Length; i++) {
			Transform clone = Instantiate (chunkPrefab, blockPos, Quaternion.identity);
			NetworkServer.Spawn (clone.gameObject);
			clone.GetComponent<Item> ().isAvailable = false;
			clone.GetComponent<LivingEntity> ().deathEvent += OnChunkDeath;
			// StartCoroutine (SetHarvestModeDelay (clone.transform.name, i));
			RpcSetHarvestMode (clone.transform.name, clone.GetComponent<LivingEntity>().entityGroupIndex , i);

			Collider mycollider = GetComponent<Collider> ();
			if (mycollider != null) {
				mycollider.enabled = false;
			}
			rig = GetComponent<Rigidbody> ();
			rig.isKinematic = true;
		}
		RpcSetStatic (transform.name);
		// Misc
	}

	[ClientRpc]
	void RpcSetHarvestMode(string cloneName, int cloneGroupIndex, int i) {
		float t = 0;
		bool pass = false;
		GameManager.instance.GetLivingEntity(cloneName, cloneGroupIndex).GetComponent<ResourceChunk> ().SetHarvestMode (i, netId);
		while (!pass) {
			t += Time.deltaTime;

			if (t >= .5f) {
				
				pass = true;
			}
		}
	}

	[ClientRpc]
	void RpcSetStatic(string _name) {
		GameManager.instance.GetEntity (_name, entityGroupIndex).gameObject.SetActive (false);
	}

	public void OnChunkDeath() {
		chunkCount--;

		if (chunkCount <= 0) {
			DestroyEntity ();
		}
	}
}
