using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {

	public static WorldManager instance;
	[HideInInspector]
	public Transform spawnedItemHolder;

	void Awake() {
		instance = this;

		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name == "SpawnedItems") {
				spawnedItemHolder = transform.GetChild (i);
			}
		}
	}
}
