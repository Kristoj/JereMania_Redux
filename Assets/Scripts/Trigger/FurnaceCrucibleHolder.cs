using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceCrucibleHolder : ChildLivingEntity {
	
	public CrucibleSlot[] crucibleSlots = new CrucibleSlot[4];

	// Called when player attacks this object with a equipment
	public override void OnTakeDamage() {
		base.OnTakeDamage ();
		Player localPlayer = GameManager.GetLocalPlayer ();
		if (localPlayer != null) {
			GunController gunController = localPlayer.GetComponent<GunController> ();
			if (gunController.currentEquipment.entityName == "Crucible") {
				parentEntity.SendMessage ("SignalCrucibleAdd", localPlayer.transform.name);
			}
		}
	}

	// If server accepeted our signal then parent the new crucible to this object
	public void AddCrucible(string crucibleName, int entityGroup) {
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

		public void OnCrucibleRemove() {
			hasCrucible = false;
			crucible = null;
		}

		public void OnCrucibleAdd (Furnace fur) {
			crucible.furnace = fur;
			hasCrucible = true;
		}
	}
}
