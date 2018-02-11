using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentLibrary : MonoBehaviour {

	public FarmPlant[] farmPlant;
	public Equipment[] equipmentList;
	public static EquipmentLibrary instance;

	void Start() {
		instance = this;
	}

	public Equipment GetEquipment (string _name) {
		foreach (Equipment e in equipmentList) {
			if (e.objectName == _name) {
				return e;
			}
		}
		return null;
	}

	[System.Serializable]
	public class FarmPlant {
		public string plantName = "Potato";
		public float growTime = 50;
		public int growStages = 3;
		public Vector2 dropCountMinMax = new Vector2 (1, 3);
		public Vector2 plantSizeMinMax = new Vector2 (.2f, 1f);
		public Mesh[] growStageMeshes;
		public Resource dropResource;
	}

	public void ConsumeItem(string itemName, string consumer) {
		// Mushroom
		if (itemName == "Mushroom") {
			GameManager.GetCharacter (consumer).GetComponent<PlayerStats>().AddFatique (10);
		}
		// Potato
		if (itemName == "Potato") {
			GameManager.GetCharacter (consumer).GetComponent<PlayerStats>().AddFatique (7);
		}
		// Carrot
		if (itemName == "Carrot") {
			GameManager.GetCharacter (consumer).GetComponent<PlayerStats> ().AddFatique (16);
		}
		// Food Ratio
		if (itemName == "Food_Ratio") {
			GameManager.GetCharacter (consumer).GetComponent<PlayerStats>().AddFatique (100);
		}
	}
}

