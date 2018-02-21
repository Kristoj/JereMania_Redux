using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Cavespawner : MonoBehaviour {

	public int maxCaves;
	public int minCaves;
	public Transform[] caveStraight;
	public Transform[] caveT;
	public Transform[] caveLLeft;
	public Transform[] caveLRight;
	public Transform[] caveX;
	public Transform[] caveEnd;
	public List<Transform> caveChunklist = new List<Transform> ();

	int currCaves = 0;
	int caveAmount;
	int caveType;

	bool spawnPoint1;
	bool spawnPoint2;
	bool spawnPoint3;


	// Use this for initialization
	void Start () {
		caveChunklist.Add (transform);
		caveAmount = Random.Range (minCaves, maxCaves);
		Debug.Log(caveAmount);

		caveSpawn ();
		currCaves++;

		for ( int i=0 ; i < caveAmount; i++) {

			caveSpawn ();
			currCaves++;
			}




	}


	void caveSpawn() { 

		caveType = Random.Range (1, 2);

		if (caveType == 1) {
			
			Transform clone = Instantiate (caveStraight[0], caveChunklist[currCaves].position + new Vector3(0, 0, 13.5f), caveChunklist[currCaves].rotation, transform);
			caveChunklist.Add (clone);
		
		}


		else if (caveType == 2) {

			//Transform clone = Instantiate (caveT[0], caveChunklist[currCaves].position + new Vector3(0, 0, 13.5f), caveChunklist[currCaves].rotation, transform);
			//caveChunklist.Add (clone);
		
		}

		/*
		else if (caveType == 3) {

			Transform clone = Instantiate (caveLLeft[0], transform.position + new Vector3(13.5f, 0, 0), Quaternion.identity, transform);

		}

		else if (caveType == 4) {

			Transform clone = Instantiate (caveLRight[0], transform.position + new Vector3(13.5f, 0, 0), Quaternion.identity, transform);

		}

		else if (caveType == 5) {


			Transform clone Instantiate (caveX[0], transform.position + new Vector3(13.5f, 0, 0), Quaternion.identity, transform);
		}
		*/
	
	}

	// Update is called once per frame
	void Update () {
		
	}
}
