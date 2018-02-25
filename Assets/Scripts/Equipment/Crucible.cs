using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Crucible : Equipment {

	public float matterTemperature = 0;
	public float furnaceTemperature = 0;
	private Transform oreMesh;
	private Transform moltenMatterObject;
	private Vector3 moltenMatterObjectOriginalScale;
	[HideInInspector]
	public Furnace furnace;

	private Coroutine meltCoroutine;

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
			}
			if (transform.GetChild (i).name == "Molten_Ore_Mesh") {
				moltenMatterObject = transform.GetChild (i);
				moltenMatterObjectOriginalScale = moltenMatterObject.transform.localScale;
			}
		}
	}

	[ClientRpc]
	public void RpcSetFurnaceMode() {
		rig = GetComponent<Rigidbody> ();
		rig.isKinematic = true;
	}

	// Called when crucible is in a crucible slot in a furnace
	public void StartMelting(Furnace fur) {
		furnace = fur;
		if (meltCoroutine == null && mineral != null) {
			meltCoroutine = StartCoroutine (UpdateCrucibleTemperature ());
		}
	}

	IEnumerator UpdateCrucibleTemperature () {
		while (furnace.isBurning || matterTemperature > 0) {
			// Increment matter temperature
			matterTemperature = Mathf.MoveTowards (matterTemperature, furnace.temperature, furnace.temperature / (furnace.temperatureEfficiency * .01f));
			// Clamp matter temperature
			matterTemperature = Mathf.Clamp (matterTemperature, 0, mineral.meltingPoint);
			yield return null;
		}
	}

	public override void OnEntityHit(string playerName, string sourceEquipmentName) {
		if (EquipmentLibrary.instance.GetEquipment(sourceEquipmentName).GetComponent<Mineral>() != null) {
			RpcAddOre (playerName);
		}
	}

	[ClientRpc]
	void RpcAddOre(string playerName) {
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<GunController> ().EquipEquipment (null, false, 0);
		}

		oreMesh.gameObject.SetActive (true);
	}
}
