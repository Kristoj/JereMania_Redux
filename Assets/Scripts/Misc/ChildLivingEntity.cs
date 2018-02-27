using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildLivingEntity : ChildEntity {

	public virtual void OnClientTakeDamage(string playerName) {
		
	}

	public virtual void OnServerTakeDamage(string playerName, string sourceEquipmentName) {

	}
}
