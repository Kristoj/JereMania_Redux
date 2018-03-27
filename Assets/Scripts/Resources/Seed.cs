using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Seed : Resource {

	public string seedName = "Potato";
	public LayerMask seedPlantMask;

	public override void SetHitMask() {
		myHitMask = seedPlantMask;
	}

	public override void OnServerEntityHit (string victimName, int victimGroup, string sourcePlayer) {
		FarmPatch farmPatch = GameManager.instance.GetEntity (victimName, victimGroup).GetComponent<FarmPatch> ();
		if (farmPatch != null && !farmPatch.hasPlant) {
			CmdPlantSeed (victimName, victimGroup);
			player.weaponController.EquipEquipment("", entityGroupIndex , false, 0);
		}
	}

	[Command]
	void CmdPlantSeed(string victimId , int victimGroup) {
		FarmPatch farmPatch = GameManager.instance.GetLivingEntity (victimId, victimGroup).GetComponent<FarmPatch> ();
		if (farmPatch != null && !farmPatch.hasPlant) {
			farmPatch.PlantSeed(seedName);
		}
	}
}
