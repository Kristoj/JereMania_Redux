using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatTrigger : Interactable {

	public Seat masterSeat;

	public override void OnStartInteraction(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnStartInteraction (masterId);
		}
	}

	public override void OnExit(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnExit (masterId);
		}
	}
}
