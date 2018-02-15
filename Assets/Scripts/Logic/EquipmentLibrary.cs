using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentLibrary : MonoBehaviour {

	public FarmPlant[] farmPlant;
	public List<Equipment> equipmentList = new List<Equipment>();
	public static EquipmentLibrary instance;

	void Awake() {
		GetComponent<AutomaticObjectAssignerScript> ().AssignObjects ();
	}

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
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().FatiqueAdd (10);
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().StaminaAdd (60);
		}
		// Potato
		if (itemName == "Potato") {
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().FatiqueAdd (7);
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().StaminaAdd (50);
		}
		// Carrot
		if (itemName == "Carrot") {
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats> ().FatiqueAdd (16);
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().StaminaAdd (80);
		}
		// Food Ratio
		if (itemName == "Food_Ratio") {
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().FatiqueAdd (100);
			GameManager.GetPlayerByName (consumer).GetComponent<PlayerStats>().StaminaAdd (100);
		}
	}

	public static void GetEntityByName(string entityName, int entityGroupIndex) {

	}
}

