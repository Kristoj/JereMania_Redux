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
		RpcSetStatic ();
		// Misc
	}

	[ClientRpc]
	void RpcSetHarvestMode(string cloneName, int cloneGroupIndex, int i) {
		LivingEntity rc = GameManager.instance.GetLivingEntity (cloneName, cloneGroupIndex);
		if (rc != null) {
			rc.GetComponent<ResourceChunk> ().SetHarvestMode (i, netId);
		}
	}

	[ClientRpc]
	void RpcSetStatic() {
		this.gameObject.SetActive(false);
	}

	public void OnChunkDeath(string sourcePlayer) {
		chunkCount--;

		if (chunkCount <= 0) {
			DestroyEntity (sourcePlayer);
		}
	}
}
