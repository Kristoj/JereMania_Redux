using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChoppingBlock : NetworkBehaviour {

	public Vector3 setupOffset;
	private bool hasWoodBlock = false;

	void OnTriggerEnter (Collider c) {
		if (isServer) {
			DynamicResourceEntity dynamicEntity = c.GetComponent<DynamicResourceEntity> ();
			//Debug.Log (block.staticBlock
			if (dynamicEntity != null && dynamicEntity.isAvailable) {
				if (!hasWoodBlock) {
					dynamicEntity.SetupChunks (transform.position + setupOffset);
					dynamicEntity.deathEvent += ResetChoppingBlock;
					hasWoodBlock = true;
				}
			}
		}
	}

	void ResetChoppingBlock(string sourcePlayer) {
		hasWoodBlock = false;
	}
}
