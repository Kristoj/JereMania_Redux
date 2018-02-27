using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMenuItemSpawning : MonoBehaviour {
	
	SpawnMenu spawnMenu;

	public void OnClick(){

		spawnMenu = GameManager.GetLocalPlayer ().GetComponent<SpawnMenu> ();
		int indexNumber = int.Parse (gameObject.name);
		spawnMenu.ButtonClicked (indexNumber);

		}
	
	}
