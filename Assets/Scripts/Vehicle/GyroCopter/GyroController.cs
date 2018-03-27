using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController : Seat {

	private bool isLooking = false;
	private Player seatOwner = null;
	private bool isSeated = false;

	public void OnEnter(string masterId){
		if(seatOwner = null){
			print (masterId);
			seatOwner = GameManager.GetPlayerByName (masterId);
			base.OnClientStartInteraction (masterId);
			GameManager.GetLocalPlayer().GetComponent<PlayerController>().cameraEnabled = false;
			isSeated = true;
		}
	}

	public void OnExit(string masterId){
		seatOwner = null;
		base.OnExit (masterId);
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
