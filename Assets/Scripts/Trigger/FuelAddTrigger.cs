using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelAddTrigger : ChildLivingEntity {

	private Fireplace fireplace;

	void Start() {
		if (parentEntity != null) {
			fireplace = parentEntity.GetComponent<Fireplace>();
		}
	}

	public override void OnServerTakeDamage(string playerName, string sourceEquipmentName) {
		// Add fuel only if fireplace needs it
		if (fireplace != null && fireplace.fuel < fireplace.maxFuel) {
			// Get reference to the player who added fuel
			GunController gunController = GameManager.GetPlayerByName(playerName).GetComponent<GunController> ();
			if (gunController != null && gunController.currentEquipment.entityName == "Wood") {
				
				// Check how much fuel we want to add
				int fuelToAdd = 0;
				switch (sourceEquipmentName) {
				case ("Wood"):
					fuelToAdd = 20;
					break;
				default:
					break;
				}

				// Add fuel
				fireplace.AddFuel (fuelToAdd);

				// Destroy players equipment
				if (gunController.currentEquipment != null) {
					gunController.DestroyCurrentEquipment (true);
				}
			}
		}
	}
}
