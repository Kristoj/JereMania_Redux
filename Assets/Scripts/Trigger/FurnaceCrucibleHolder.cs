using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceCrucibleHolder : ChildLivingEntity {
	public int crucibleSlots = 4;
	public int crucibleCount = 0;
	private ChildDoor doorClass;

	void Start() {
		doorClass = GetComponent<ChildDoor> ();
	}

	public override void OnTakeDamage() {
		Player localPlayer = GameManager.GetLocalPlayer ();
		if (localPlayer != null) {
			GunController gunController = localPlayer.GetComponent<GunController> ();
			if (gunController.currentEquipment.entityName == "Crucible" && doorClass != null) {
				parentEntity.SendMessage ("SignalCrucibleAdd", localPlayer.transform.name);
			}
		}
	}

	public void AddCrucible(string crucibleName, int entityGroup) {
		Crucible c = GameManager.instance.GetEntity (crucibleName, entityGroup)as Crucible;
		if (c != null) {
			c.transform.SetParent (transform);
		}
	}
}
