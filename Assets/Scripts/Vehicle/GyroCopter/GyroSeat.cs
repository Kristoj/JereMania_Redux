using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroSeat : Seat {
	public bool isSeated = false;
	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);
		GameManager.GetLocalPlayer().GetComponent<PlayerController>().cameraEnabled = false;
		isSeated = true;
	}

	public override void OnExit(string masterId) {
		base.OnExit (masterId);
		GameManager.GetLocalPlayer ().GetComponent<PlayerController> ().cameraEnabled = true;
		isSeated = false;
	}

	void Update(){
		if(isSeated && Input.GetKeyDown(KeyCode.LeftAlt)){
		GameManager.GetLocalPlayer ().GetComponent<PlayerController> ().cameraEnabled = true;
		}
	}

}
