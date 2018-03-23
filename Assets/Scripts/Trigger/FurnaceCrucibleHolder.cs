using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceCrucibleHolder : ChildLivingEntity {
	
	public CrucibleSlot[] crucibleSlots = new CrucibleSlot[4];

	// Called when player attacks this object with a equipment
	public override void OnServerTakeDamage(string playerName, string sourceEquipmentName) {
		Player sourcePlayer = GameManager.GetPlayerByName (playerName);
		if (sourcePlayer != null) {
			GunController gunController = sourcePlayer.GetComponent<GunController> ();
			if (gunController.currentEquipment != null && gunController.currentEquipment.entityName == "Crucible") {
				(parentEntity as Furnace).SignalCrucibleAdd (playerName, sourceEquipmentName, gunController.currentEquipment.entityGroupIndex);
			}
		}
	}

	// If server accepeted our signal then parent the new crucible to this object
	public void OnClientAddCrucible(string crucibleName, int entityGroup) {
		Crucible c = GameManager.instance.GetEntity (crucibleName, entityGroup)as Crucible;
		if (c != null) {
			c.transform.SetParent (transform);
		}
	}

	public Vector3 GetSlotPos(CrucibleSlot cs) {
		return transform.position + parentEntity.transform.TransformVector (cs.slotOffset);
	}

	// Get empty crucible slot if any
	public CrucibleSlot GetEmptySlot() {
		foreach (CrucibleSlot c in crucibleSlots) {
			if (!c.hasCrucible) {
				return c;
			}
		}
		return null;
	}

	// Crucible class that stores information about every crucible slot
	[System.Serializable]
	public class CrucibleSlot {
		public bool hasCrucible = false;
		public Vector3 slotOffset;
		public Crucible crucible;

		public void OnCrucibleRemove(string targetPlayer) {
			hasCrucible = false;
			crucible.StopAllCoroutines ();
			crucible = null;
		}

		public void OnCrucibleAdd (Furnace fur) {
			crucible.furnace = fur;
			hasCrucible = true;
		}
	}
}
