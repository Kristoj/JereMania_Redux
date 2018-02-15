using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Seed : MeleeWeapon {

	public string seedName = "Potato";
	public LayerMask seedPlantLayer;

	public override void SetHitMask() {
		myHitMask = seedPlantLayer;
	}

	public override void TakeDamage (string victimId, int targetGroup, string playerId) {
		FarmPatch farmPatch = GameManager.instance.GetEntity (victimId, targetGroup).GetComponent<FarmPatch> ();
		if (!farmPatch.hasPlant) {
			CmdPlantSeed (victimId, playerId);
			weaponController.DestroyCurrentEquipment ();
		}
	}

	[Command]
	void CmdPlantSeed(string victimId, string playerId) {
		FarmPatch farmPatch = GameManager.GetCharacterByName (victimId).GetComponent<FarmPatch> ();
		if (!farmPatch.hasPlant) {
			farmPatch.PlantSeed(seedName);
		}
	}
}
