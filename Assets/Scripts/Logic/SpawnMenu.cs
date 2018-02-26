using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMenu : MonoBehaviour {
	
	public float slotSize = 30f;
	public float slotGap = 0.25f;

	public RectTransform slot;
	public RectTransform menuBackGround;
	public List<SpawnMenuSlot> slots = new List<SpawnMenuSlot>();


	float slotAmountX = 10;
	float slotSlotAmountY = 3;
	int equipmentAmount;

	bool menuOpen = false;

	// Use this for initialization
	void Start () {
		
		ScanPrefabs ();
		SetupMenu ();

	}

	//Scans Prefabs to show in menu
	void ScanPrefabs(){

		object[] equipments = Resources.LoadAll ("Prefabs/Equipment", typeof(Equipment));
		equipmentAmount = equipments.Length;

	}

	void SetupMenu(){
		
		//Get start pos for slots
		Vector3 startPos;

}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F2)){

			OpenMenu();
		
		}
	
	}

			void OpenMenu() {
		bool menuOpen = true;

	
	}

}