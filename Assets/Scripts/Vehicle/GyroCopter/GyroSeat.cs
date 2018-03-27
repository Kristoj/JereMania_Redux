using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroSeat : Seat {
	// On enter
	public override void OnClientStartInteraction(string masterId) {
		transform.parent.GetComponent<GyroController> ().OnEnter(masterId);
	}
	// On exit
	public override void OnExit(string masterId) {
		transform.parent.GetComponent<GyroController> ().OnExit(masterId);
	}
}
