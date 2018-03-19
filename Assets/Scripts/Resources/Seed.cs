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

	public override void TakeDamage (string victimId, int victimGroup, string playerId) {
		FarmPatch farmPatch = GameManager.instance.GetEntity (victimId, victimGroup).GetComponent<FarmPatch> ();
		if (!farmPatch.hasPlant) {
			CmdPlantSeed (victimId, victimGroup);
			weaponController.EquipEquipment("", entityGroupIndex , false, 0);
		}
	}

	[Command]
	void CmdPlantSeed(string victimId , int victimGroup) {
		FarmPatch farmPatch = GameManager.instance.GetLivingEntity (victimId, victimGroup).GetComponent<FarmPatch> ();
		if (!farmPatch.hasPlant) {
			farmPatch.PlantSeed(seedName);
		}
	}
}
