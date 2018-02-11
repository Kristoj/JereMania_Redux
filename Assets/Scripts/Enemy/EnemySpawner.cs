using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

	public Enemy enemyToSpawn;
	public Transform spawn;

	
	public void CmdSpawnEnemy() {
		Debug.Log ("spawn");
		StartCoroutine (CmdSpawn ());
	}
		
	IEnumerator CmdSpawn() {
		yield return new WaitForSeconds (.5f);
		Enemy clone = Instantiate (enemyToSpawn, spawn.transform.position, spawn.transform.rotation)as Enemy;
		NetworkServer.Spawn (clone.gameObject);
	}
}
