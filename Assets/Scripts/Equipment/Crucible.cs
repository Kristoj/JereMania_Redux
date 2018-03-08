using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Crucible : Equipment {

	public float matterTemperature = 0;
	public float meltTime = 0;
	private Transform oreMesh;
	private Vector3 oreMeshOriginalPos;
	private Transform moltenMatterObject;
	private Vector3 moltenMatterObjectOriginalScale;
	private Vector3 moltenMatterObjectTargetScale;
	public Furnace furnace;

	private Coroutine tempUpdateCoroutine;
	private Coroutine serverVisualCoroutine;
	private Coroutine clientVisualCoroutine;

	// These should be somewhere else...
	public float oreMeltingTemperature = 1000;
	public float oreMeltingProgress = 0;

	// Mineral inside the crucible
	public Mineral mineral;

	public override void Start() {
		base.Start ();

		// Get child references
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name == "Ore_Mesh") {
				oreMesh = transform.GetChild (i);
				oreMeshOriginalPos = oreMesh.transform.localPosition;
			}
			if (transform.GetChild (i).name == "Molten_Matter_Mesh") {
				moltenMatterObject = transform.GetChild (i);
				moltenMatterObjectOriginalScale = moltenMatterObject.transform.localScale;
			}
		}
	}

	// Called when player attacks this crucible with mineral in his hand
	public override void OnEntityHit(string playerName, string sourceEquipmentName) {
		Mineral sourceMineral = EquipmentLibrary.instance.GetEquipment (sourceEquipmentName) as Mineral;
		if (sourceMineral != null && mineral == null) {
			RpcAddOre (playerName);

			// Set mineral to the crucible
			mineral = sourceMineral;
			meltTime = 0;

			if (tempUpdateCoroutine != null) {
				StopCoroutine (tempUpdateCoroutine);
			}
			StartCoroutine (UpdateMineralTemperature ());
		}
	}

	[ClientRpc]
	// Add ore mesh to the crucible and for the player who placed it there remove it from his inventory
	void RpcAddOre(string playerName) {
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<GunController> ().DestroyCurrentEquipment (true);
		}

		oreMesh.gameObject.SetActive (true);
	}

	// Called when crucible is in a crucible slot in a furnace.. Starts melting the ore...
	public void StartMelting(Furnace fur) {
		furnace = fur;
		if (tempUpdateCoroutine != null) {
			StopCoroutine (tempUpdateCoroutine);
		}

		if (mineral != null) {
			tempUpdateCoroutine = StartCoroutine (UpdateMineralTemperature ());
		}
	}

	// Update mineral temperature inside the crucible 
	IEnumerator UpdateMineralTemperature () {
		moltenMatterObject.gameObject.SetActive (true);
		moltenMatterObject.transform.localScale = new Vector3 (moltenMatterObjectOriginalScale.x, moltenMatterObjectOriginalScale.y * (meltTime / mineral.meltTime), moltenMatterObjectOriginalScale.z);

		if (furnace == null) {
			yield break;
		}
		// Update matter temperature
		while (furnace.isBurning || matterTemperature > 0) {
			// Increment matter temperature
			matterTemperature = Mathf.MoveTowards (matterTemperature, furnace.temperature, furnace.temperature / (furnace.temperatureEfficiency * .01f) * Time.deltaTime);
			// Clamp matter temperature
			matterTemperature = Mathf.Clamp (matterTemperature, 0, mineral.meltingPoint);

			if (matterTemperature >= mineral.meltingPoint) {

				// Increment melt time
				meltTime += Time.deltaTime;
				moltenMatterObject.transform.localScale = new Vector3 (moltenMatterObjectOriginalScale.x, moltenMatterObjectOriginalScale.y * (meltTime / mineral.meltTime), moltenMatterObjectOriginalScale.z);
				oreMesh.transform.localPosition = oreMeshOriginalPos - (transform.up * (.15f * (meltTime / mineral.meltTime)));

				// If 
				if (serverVisualCoroutine == null) {
					serverVisualCoroutine = StartCoroutine (UpdateMoltenVisuals ());
				}

				if (meltTime >= mineral.meltTime) {
					FinishMelting ();
					yield break;
				}
			}
			yield return null;
		}
	}

	// Server updates molten matter scale for every client
	IEnumerator UpdateMoltenVisuals() {
		float updateRate = 4f;

		while (matterTemperature >= mineral.meltingPoint) {
			RpcUpdateMoltenVisuals (moltenMatterObject.localScale);
			yield return new WaitForSeconds (updateRate);
		}

	}

	[ClientRpc]
	// Update the target scale for clients... If we aren't updating our visuals, start updating them
	void RpcUpdateMoltenVisuals(Vector3 targetScale) {
		if (!isServer) {
			moltenMatterObjectTargetScale = targetScale;
			if (clientVisualCoroutine == null) {
				clientVisualCoroutine = StartCoroutine (ClientUpdateMoltenVisuals ());
			}
		}
	}

	// Update molten matter visuals for every client... If our molten matter gameobjects size is the same as the servers, exit this coroutine
	IEnumerator ClientUpdateMoltenVisuals() {
		// First time setup
		moltenMatterObject.gameObject.SetActive (true);
		moltenMatterObject.transform.localScale = new Vector3 (moltenMatterObjectOriginalScale.x, moltenMatterObjectOriginalScale.y, 0);
		// Update visuals while haven't reached the target scale
		while (moltenMatterObject.transform.localScale != moltenMatterObjectTargetScale) {
			moltenMatterObject.transform.localScale = Vector3.Lerp (moltenMatterObject.transform.localScale, moltenMatterObjectTargetScale, .3f * Time.deltaTime);
			yield return null;
		}
		Debug.Log ("Client End");
	}
	// Called on the server when server finished melting the ore
	void FinishMelting() {
		RpcFinishMelting ();
	}

	[ClientRpc]
	// Called on the client when server finished melting the ore
	void RpcFinishMelting() {
		oreMesh.gameObject.SetActive (false);
	}

	[ClientRpc]
	// Called when crucible is added to a furnace...
	public void RpcSetFurnaceMode(Vector3 spawnPos, Vector3 spawnEulers) {
		// Set our state to static and orientate this crucible accordingly
		rig = GetComponent<Rigidbody> ();
		rig.isKinematic = true;
		transform.position = spawnPos;
		transform.eulerAngles = spawnEulers;
	}
}