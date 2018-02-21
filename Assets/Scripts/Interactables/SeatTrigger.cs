using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatTrigger : Interactable {

	public Seat masterSeat;

	public override void OnClientStartInteraction(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnClientStartInteraction (masterId);
		}
	}

	public override void OnExit(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnExit (masterId);
		}
	}
}
