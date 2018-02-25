using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentLibrary : MonoBehaviour {

	public FarmPlant[] farmPlant;
	public List<Equipment> equipmentList = new List<Equipment>();
	public static EquipmentLibrary instance;

	void Awake() {
		//GetComponent<AutomaticObjectAssignerScript> ().AssignObjects ();
	}

	void Start() {
		instance = this;
	}

	public Equipment GetEquipment (string _entityName) {
		foreach (Equipment e in equipmentList) {
			if (e.entityName == _entityName) {
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
		PlayerStats consumerStats = GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>();
		if (itemName == "Mushroom") {
			consumerStats.FatiqueAdd (.5f);
			consumerStats.StaminaAdd (60);
			consumerStats.HungerAdd (17);
		}
		// Potato
		else if (itemName == "Potato") {
			consumerStats.FatiqueAdd (.75f);
			consumerStats.StaminaAdd (50);
			consumerStats.HungerAdd (15);
		}
		// Carrot
		else if (itemName == "Carrot") {
			consumerStats.FatiqueAdd (1);
			consumerStats.StaminaAdd (80);
			consumerStats.HungerAdd (28);
		}
		// Food Ratio
		else if (itemName == "Food_Ratio") {
			consumerStats.FatiqueAdd (3);
			consumerStats.StaminaAdd (100);
			consumerStats.HungerAdd (100);
		}
	}
}

