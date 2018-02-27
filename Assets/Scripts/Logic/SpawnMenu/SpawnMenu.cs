using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnMenu : NetworkBehaviour {
	
	public float itemSlotSize = 30f;
	public float itemSlotGap = 0.25f;


	public RectTransform itemSlot;
	public RectTransform menuBackground;
	public RectTransform spawnMenuScreen;
	public List<RectTransform> slotList = new List<RectTransform>();
	public List<Equipment> equipmentList = new List<Equipment>();
	private PlayerController playerController;

	protected int currEquipments = 0;

	float slotAmountX = 10;
	int equipmentAmount;

	bool menuOpen = false;

	// Use this for initialization
	void Start () {
		playerController = GetComponent<PlayerController> ();
		SetupMenu ();

	}

	//Scans Prefabs to show in menu


	void SetupMenu(){
		//Scan Lists
		equipmentList = EquipmentLibrary.instance.equipmentList;
		equipmentAmount = EquipmentLibrary.instance.equipmentList.Capacity;
		//Get start pos for slots
		Vector3 startPosition;
		startPosition.x = -((menuBackground.sizeDelta.x * 0.5f) - itemSlotSize);
		startPosition.y = ((menuBackground.sizeDelta.y * 0.5f) - itemSlotSize);
		int currEquipments = 0;
		int currLine = 0;

		while (currEquipments < equipmentAmount){
			
			currLine--;

			for (int i = 0; i < slotAmountX; i++) {
				
				currEquipments++;

				if (currEquipments > equipmentAmount) {
					break;
				}

				RectTransform itemSlotClone = Instantiate (itemSlot, menuBackground.position, menuBackground.rotation, menuBackground) as RectTransform;
				itemSlotClone.sizeDelta = new Vector2 (itemSlotSize, itemSlotSize);
				Vector3 itemSlotPosition = new Vector3(startPosition.x + (itemSlotClone.sizeDelta.x * i + itemSlotGap), startPosition.y + ((itemSlotClone.sizeDelta.y * currLine - itemSlotGap)), 0);
				itemSlotClone.transform.localPosition = itemSlotPosition;
				slotList.Add (itemSlotClone);
				itemSlotClone.gameObject.name = ("" + currEquipments);


			}

		} 
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F2)){

			OpenMenu ();

		}
	
	}

			void OpenMenu() {
		
		menuOpen = !menuOpen;

		switch (menuOpen) {

		case true:
			
			spawnMenuScreen.gameObject.SetActive (true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			playerController.SetPlayerEnabled (false);
			break;

		case false:
			
			spawnMenuScreen.gameObject.SetActive (false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			playerController.SetPlayerEnabled (true);
			break;
		}
	}

	public void ButtonClicked (int itemNumber) {
		
		if (isServer) {
			
			SpawnItem (itemNumber);
		
		}
			else {
			
			CmdClientSignalSpawn (itemNumber);

		}
	}

	[Command]
	void CmdClientSignalSpawn (int itemNumber){
		
		SpawnItem(itemNumber);

	}
		
	void SpawnItem(int itemNumber) {
		
		Equipment Clone = Instantiate (equipmentList [itemNumber], transform.position + transform.forward * 2 + transform.up, transform.rotation, null) as Equipment;
		NetworkServer.Spawn (Clone.gameObject);

		}
}