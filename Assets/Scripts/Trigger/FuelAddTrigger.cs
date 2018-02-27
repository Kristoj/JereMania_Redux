﻿using System.Collections;
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
		if (fireplace != null && fireplace.fuel < fireplace.maxFuel) {
			Equipment e = GameManager.GetPlayerByName(playerName).GetComponent<GunController> ().currentEquipment;
			if (e != null) {
				fireplace.AddFuel (GameManager.GetLocalPlayer().name, e.entityName);
			}
		}
	}
}
