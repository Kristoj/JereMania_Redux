using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Cavespawner : MonoBehaviour {

	public int maxCaves;
	public int minCaves;
	public CaveChunk[] caveStraight;
	public CaveChunk[] caveLLeft;
	public CaveChunk[] caveLRight;
	public CaveChunk[] caveT;
	public CaveChunk[] caveX;
	public CaveChunk[] caveEnd;

	[HideInInspector]
	public List<CaveChunk> caveChunkList = new List<CaveChunk> ();

	int currCaves = 0;
	int caveAmount;

	//Rotation offsets 
	int offsetX;
	int offsetY;
	int offsetZ = 18;
	int offsetRot;


	// Use this for initialization
	void Start () {

		StartGenerate ();
	
	}

	public void StartGenerate () {
		
		caveChunkList.Add (GetComponent<CaveChunk> ());
		caveAmount = Random.Range (minCaves, maxCaves);
		Debug.Log ("Cave amount:" + caveAmount);

		CaveSpawn1 ();
		currCaves++;
		Debug.Log ("currCaves:" + currCaves);	

		for (int i = 0; i < caveAmount; i++) {


			//Check for previous cave chunk type

			//STRAIGHT
			if (caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Straight) {

				//Check for previous cave chunk orientation

				//Cave chunk points forward
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

					offsetZ = 18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}

				//Cave chunk points left
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

					offsetZ = 0;
					offsetX = -18;
					offsetY = 0;
					offsetRot = 270;
				}

				//Cave chunk points right
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

					offsetZ = 0;
					offsetX = 18;
					offsetY = 0;
					offsetRot = 90;
				}

				//Cave chunk points backwards
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

					offsetZ = -18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}
					
				CaveSpawn1 ();
				currCaves++;
				Debug.Log ("currCaves:" + currCaves);		
			}
				

			//LEFT
			if (caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Left) {

				//Check for previous cave chunk orientation

				//Cave chunk points forward
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

					offsetZ = 18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}

				//Cave chunk points left
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

					offsetZ = 0;
					offsetX = -18;
					offsetY = 0;
					offsetRot = 270;
				}

				//Cave chunk points right
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

					offsetZ = 0;
					offsetX = 18;
					offsetY = 0;
					offsetRot = 90;
				}

				//Cave chunk points backwards
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

					offsetZ = -18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}

				CaveSpawn2 ();
				currCaves++;
				Debug.Log ("currCaves:" + currCaves);

			}


			//RIGHT
			if (caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Right) {

				//Check for previous cave chunk orientation

				//Cave chunk points forward
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

					offsetZ = 18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}

				//Cave chunk points left
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

					offsetZ = 0;
					offsetX = -18;
					offsetY = 0;
					offsetRot = 270;
				}

				//Cave chunk points right
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

					offsetZ = 0;
					offsetX = 18;
					offsetY = 0;
					offsetRot = 90;
				}

				//Cave chunk points backwards
				if (caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

					offsetZ = -18;
					offsetX = 0;
					offsetY = 0;
					offsetRot = 0;
				}
					
				CaveSpawn3 ();
				currCaves++;
				Debug.Log ("currCaves:" + currCaves);

			}
	
		}
	}
			
			

	//spawning these if previous cave piece is left
	void CaveSpawn1 () {

		int caveType = Random.Range (1, 3);
		Debug.Log ("caveType:" + caveType);

		if (caveType == 1) {

			CaveChunk clone = Instantiate (caveStraight[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, offsetRot, 0)), transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 2) {

			CaveChunk clone = Instantiate (caveLLeft[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, offsetRot, 0)), transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 3) {

			CaveChunk clone = Instantiate (caveLRight[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, offsetRot, 0)), transform);
			caveChunkList.Add (clone);
		}



	}



	//spawning these if previous cave piece is left
	void CaveSpawn2 () {
		int caveType = Random.Range (1, 3);

		if (caveType == 1) {

			CaveChunk clone = Instantiate (caveStraight[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, 90 + offsetRot, 0)), transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 2) {

			CaveChunk clone = Instantiate (caveLLeft[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, 270 + offsetRot, 0)), transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 3) {

			CaveChunk clone = Instantiate (caveLRight[0], caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles + new Vector3(0, 90 + offsetRot, 0)), transform);
			caveChunkList.Add (clone);
		}


		else if (caveType == 4) {

			CaveChunk clone = Instantiate (caveT[0], transform.position + transform.forward * 18, Quaternion.identity, transform);
			caveChunkList.Add (clone);
		}

		else if (caveType == 5) {


			CaveChunk clone = Instantiate (caveX[0], transform.position + transform.forward * 18, Quaternion.identity, transform);
			caveChunkList.Add (clone);
		}


	}


	//spawning these if previous cave piece is right
	void CaveSpawn3 () {
		int caveType = Random.Range (1, 4);

		if (caveType == 1) {

			CaveChunk clone = Instantiate (caveStraight[0], caveChunkList[currCaves].transform.position + new Vector3(13.5f, 0, 13.5f), caveChunkList[currCaves].transform.rotation, transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 2) {

			CaveChunk clone = Instantiate (caveLLeft[0], caveChunkList[currCaves].transform.position + new Vector3(0, 0, 13.5f), Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles += new Vector3(0, 180, 0))  , transform);
			caveChunkList.Add (clone);

		}


		else if (caveType == 3) {

			CaveChunk clone = Instantiate (caveLRight[0], transform.position + new Vector3(0, 0, 13.5f), Quaternion.Euler(caveChunkList[currCaves].transform.eulerAngles += new Vector3(0, 270, 0)), transform);
			caveChunkList.Add (clone);
		}


		else if (caveType == 4) {

			CaveChunk clone = Instantiate (caveT[0], transform.position + new Vector3(0, 0, 13.5f), Quaternion.identity, transform);
			caveChunkList.Add (clone);

		}

		else if (caveType == 5) {


			CaveChunk clone = Instantiate (caveX[0], transform.position + new Vector3(0, 0, transform.position.z * 0.5f), Quaternion.identity, transform);
			caveChunkList.Add (clone);
		}



	}


	public void ClearCave (){

		foreach (CaveChunk ch in caveChunkList) {

			if (ch.transform.name != transform.name) {
				
				DestroyImmediate (ch);
			
			}
		}

		caveChunkList.Clear ();
		currCaves = 0;

	}


	// Update is called once per frame
	void Update () {
		

	}
}
