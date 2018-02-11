using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FarmArea : NetworkBehaviour {

	public int patchesPerRow = 2;
	public int rowCount = 2;
	public float sizeX = 2f;
	public float sizeY = 2f;
	public float yOffset = .5f;
	public List<FarmPatch> patches = new List<FarmPatch>();
	public FarmPatch patchPrefab;

	void Start() {
		if (isServer) {
			SpawnPatches ();
		}
	}

	void SpawnPatches() {
		//FarmPatch clone = Instantiate (patchPrefab, transform.position, transform.rotation) as FarmPatch;
		//patches.Add (clone);

		Vector3 startPos = transform.position;
		startPos += transform.up * yOffset;
		startPos -= transform.right * ((sizeX / 2));
		startPos += transform.forward * ((sizeY / 2));
		for (int i = 0; i < rowCount; i++) {
			for (int j = 0; j < patchesPerRow; j++) {
				FarmPatch clone = Instantiate (patchPrefab, startPos + (transform.right * (sizeX / patchesPerRow) * j) - (transform.forward * (sizeY / rowCount) * i), transform.rotation) as FarmPatch;
				clone.transform.name = "Patch " + j;
				patches.Add (clone);
				NetworkServer.Spawn (clone.gameObject);
			}
		}
	}
}
