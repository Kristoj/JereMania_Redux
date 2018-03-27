using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController : ParentEntity {

	private bool isLooking = false;
	private Player seatOwner = null;
	private bool isSeated = false;

	public void OnEnter(string sourcePlayer){
		if (seatOwner == null){
			print (sourcePlayer);
			seatOwner = GameManager.GetPlayerByName (sourcePlayer);
			GameManager.GetLocalPlayer().GetComponent<PlayerController>().cameraEnabled = false;
			isSeated = true;
		}
	}

	public void OnExit(string sourcePlayer){
		seatOwner = null;
		GameManager.GetLocalPlayer ().GetComponent<PlayerController> ().cameraEnabled = true;
		isSeated = false;
	}



	void Update () {

		if(isSeated){
		if(Input.GetKeyDown(KeyCode.LeftAlt) && !isLooking){
			GameManager.GetLocalPlayer ().GetComponent<PlayerController> ().cameraEnabled = true;
			isLooking = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftAlt) && isLooking){
			GameManager.GetLocalPlayer ().GetComponent<PlayerController> ().cameraEnabled = false;
			isLooking = false;
			seatOwner.transform.rotation = transform.rotation;
		}
	}

	}

}
