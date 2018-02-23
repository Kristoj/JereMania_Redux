using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceCrucibleHolder : LocalLivingEntity {
	public int crucibleSlots = 4;
	public int cruciblesInSlots = 0;
	private ChildDoor doorClass;

	void Start() {
		doorClass = GetComponent<ChildDoor> ();
	}

	public override void OnTakeDamage() {
		Player localPlayer = GameManager.GetLocalPlayer ();
		if (localPlayer != null) {
			GunController gunController = localPlayer.GetComponent<GunController> ();
			if (gunController.currentEquipment.entityName == "Crucible" && doorClass != null) {
				doorClass.parentEntity.SendMessage ("SignalCrucibleAdd");
			}
		}
	}
}
