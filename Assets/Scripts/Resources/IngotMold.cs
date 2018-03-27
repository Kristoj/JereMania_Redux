using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotMold : Equipment {

	public float coolingRate = 5f;
	private Mineral ingotMineral;
	private GameObject ingotMesh;
	private float ingotTemperature;

	public override void Start() {
		base.Start();
		ingotMesh = transform.GetChild (0).gameObject;
	}

	public override void OnEntityHit(string sourcePlayer, string sourceEquipmentName) {
		base.OnEntityHit (sourcePlayer, sourceEquipmentName);

		// Retrun if we already have a ingot inside
		if (ingotMineral != null) {
			return;
		}

		// If we can't find ingot mesh gameobject don't go any further
		if (ingotMesh == null) {
			Debug.LogError ("Could not find ingot mesh gameobject!");
			return;
		}

		// Get reference to the player and destroy his crucible mineral, and add it to this ingot.
		Player player = GameManager.GetPlayerByName (sourcePlayer);
		if (player.weaponController.currentEquipment as Crucible != null && (player.weaponController.currentEquipment as Crucible).mineral != null) {
			if ((player.weaponController.currentEquipment as Crucible).isMelted ()) {
				// Add mineral to this mold and remove mineral from the crucible
				ingotMineral = ItemDatabase.instance.GetEquipment((player.weaponController.currentEquipment as Crucible).mineral.entityName) as Mineral;
				(player.weaponController.currentEquipment as Crucible).RemoveMineral();

				// Mold state
				ingotTemperature = ingotMineral.meltingPoint;
				isAvailable = false;

				// Update ingot visuals
				ingotMesh.SetActive (true);

				// Start cooldown loop
				StartCoroutine(Cooldown());
			}
		}
	}

	IEnumerator Cooldown() {
		while (ingotTemperature > 0) {
			ingotTemperature -= coolingRate * Time.deltaTime;
			yield return null;
		}
		OnFinishedCooling ();
	}

	void OnFinishedCooling() {
		isAvailable = true;
	}

	#region INTERACTIONS
	public override void OnServerStartInteraction(string sourcePlayer) {
		if (isAvailable) {
			// Pick mineral
			if (ingotMineral != null && ingotTemperature <= 0) {
				ingotMineral.OnServerStartInteraction (sourcePlayer);
				ingotMineral.OnClientStartInteraction (sourcePlayer);
			} 
			// Pick mold
			else {
				base.OnServerStartInteraction (sourcePlayer);
			}
		}
	}

	public override void OnServerStartSwap(string sourcePlayer) {
		if (isAvailable) {
			// Pick mineral
			if (ingotMineral != null && ingotTemperature <= 0) {
				ingotMineral.OnServerStartSwap (sourcePlayer);
				ingotMineral.OnClientStartSwap (sourcePlayer);
			} 
			// Pick mold
			else {
				base.OnServerStartSwap (sourcePlayer);
			}
		}
	}
	#endregion
}
