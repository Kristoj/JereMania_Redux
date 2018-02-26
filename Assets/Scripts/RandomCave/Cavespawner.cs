using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Cavespawner : NetworkBehaviour {
	[Range (100, 400)]
	public int maxCaves;
	[Range (20, 100)]
	public int minCaves;

	public CaveChunk[] caveStraight;
	public CaveChunk[] caveLLeft;
	public CaveChunk[] caveLRight;
	public CaveChunk[] caveT;
	public CaveChunk[] caveX;
	public CaveChunk[] caveEnd;

	//[HideInInspector]
	public List<ChunckListGroup> caveChunkGroup = new List<ChunckListGroup> ();

	public class ChunckListGroup{
		
		public List<CaveChunk> caveChunkList = new List<CaveChunk> ();
	
	}
		


	int currCaves = 0;
	int caveAmount;

	//Rotation offsets 
	int offsetX;
	int offsetY;
	int offsetZ = 18;
	int offsetRot;


	// Use this for initialization
	void Start () {
		
		if (isServer) {

		StartGenerate ();
		
		}

	}
		
	



	public void StartGenerate () {
		
		caveChunkGroup.Add (new ChunckListGroup());

		caveChunkGroup[0].caveChunkList.Add (GetComponent<CaveChunk> ());

		caveAmount = Random.Range (minCaves, maxCaves);


		Debug.Log ("currCaves:" + currCaves);	
		StartCoroutine (Spawning ());

	}

	IEnumerator Spawning () {

		CaveSpawn1 ();
		currCaves++;


	for (int i = 0; i < caveAmount; i++) {

			ChunkCheck ();
			yield return null;
		}

	}
	void ChunkCheck () {
		//STRAIGHTPIECE
		if (caveChunkGroup[0].caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Straight) {

			//Check for previous cave chunk orientation

			//Cave chunk rotation 0
			if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

				offsetZ = 18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 0;


			}

			//Cave chunk rotation 90
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

				offsetZ = 0;
				offsetX = 18;
				offsetY = 0;
				offsetRot = 90;


			}

			//Cave chunk rotation 180
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

				offsetZ = -18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 180;

			}

				//Cave chunk rotation 270
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

				offsetZ = 0;
				offsetX = -18;
				offsetY = 0;
				offsetRot = 270;


			}
					
			CaveSpawn1 ();
			currCaves++;	

				
		}
		//LEFTPIECE
		else if (caveChunkGroup[0].caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Left) {

			//Check for previous cave chunk orientation

			//Cave chunk rotation 0 (points left)
			if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

				offsetZ = 0;
				offsetX = -18;
				offsetY = 0;
				offsetRot = 270;


			}

				//Cave chunk rotation 90 (points forward)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

				offsetZ = 18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 0;


			}

				//Cave chunk rotation 180 (points right)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

				offsetZ = 0;
				offsetX = 18;
				offsetY = 0;
				offsetRot = 90;


			}
					

				//Cave chunk rotation 270 (points backwards)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

				offsetZ = -18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 180;


			}
					
			CaveSpawn1 ();
			currCaves++;

		}
		//RIGHTPIERE
		else if (caveChunkGroup[0].caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.Right) {

			//Check for previous cave chunk orientation

			//Cave chunk rotation 0 (points right)
			if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

				offsetZ = 0;
				offsetX = 18;
				offsetY = 0;
				offsetRot = 90;
			}


				//Cave chunk rotation 90 (points backwards)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

				offsetZ = -18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 180;
			}

				//Cave chunk rotation 180 (points left)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

				offsetZ = 0;
				offsetX = -18;
				offsetY = 0;
				offsetRot = 270;
			}


				//Cave chunk rotation 270 (points forward)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

				offsetZ = 18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 0;
			}

			CaveSpawn1 ();
			currCaves++;
			Debug.Log ("currCaves:" + currCaves);

	

		}
		//TPIECE
		else if (caveChunkGroup[0].caveChunkList [currCaves].caveShape == CaveChunk.CaveShape.T) {
			
			CaveSpawnT();

			#region RotationCheck
			//Check for previous cave chunk orientation

			//Cave chunk rotation 0 (points left)
			if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y) {

				offsetZ = 0;
				offsetX = -18;
				offsetY = 0;
				offsetRot = 270;


			}

			//Cave chunk rotation 90 (points forward)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 90) {

				offsetZ = 18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 0;


			}

			//Cave chunk rotation 180 (points right)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 180) {

				offsetZ = 0;
				offsetX = 18;
				offsetY = 0;
				offsetRot = 90;


			}


			//Cave chunk rotation 270 (points backwards)
			else if (caveChunkGroup[0].caveChunkList [currCaves].transform.eulerAngles.y == transform.eulerAngles.y + 270) {

				offsetZ = -18;
				offsetX = 0;
				offsetY = 0;
				offsetRot = 180;

				#endregion

			}

		}

	}
			
			

	void CaveSpawn1 () {

		int caveType = Random.Range (1, 4);
		Debug.Log ("caveType:" + caveType);

		if (caveType == 1) {

			CaveChunk clone = Instantiate (caveStraight[0], caveChunkGroup[0].caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(transform.eulerAngles + new Vector3(0, offsetRot, 0)), null);
			caveChunkGroup[0].caveChunkList.Add (clone);
			NetworkServer.Spawn (clone.gameObject);
		}


		else if (caveType == 2) {

			CaveChunk clone = Instantiate (caveLLeft[0], caveChunkGroup[0].caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(transform.eulerAngles + new Vector3(0, offsetRot, 0)), null);
			caveChunkGroup[0].caveChunkList.Add (clone);
			NetworkServer.Spawn (clone.gameObject);
		}


		else if (caveType == 3) {

			CaveChunk clone = Instantiate (caveLRight[0], caveChunkGroup[0].caveChunkList[currCaves].transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.up * offsetY, Quaternion.Euler(transform.eulerAngles + new Vector3(0, offsetRot, 0)), null);
			caveChunkGroup[0].caveChunkList.Add (clone);
			NetworkServer.Spawn (clone.gameObject);
		}



	}
		


	//Spawning when last cave piece was T
	void CaveSpawnT () {
		
		 

		
	}


	public void ClearCave (){

		foreach (CaveChunk ch in caveChunkGroup[0].caveChunkList) {

			if (ch.transform.name != transform.name) {
				
				DestroyImmediate (ch);
			
			}
		}

		caveChunkGroup[0].caveChunkList.Clear ();
		currCaves = 0;


	}



	// Update is called once per frame
	void Update () {
		

	}
}
