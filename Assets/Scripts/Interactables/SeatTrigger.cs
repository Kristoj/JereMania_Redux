using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatTrigger : Interactable {

	public ChildSeat masterSeat;

	public override void OnClientStartInteraction(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnClientStartInteraction (masterId);
		}
	}

	public override void OnServerExit(string masterId) {
		if (masterSeat != null) {
			masterSeat.OnServerExit (masterId);
		}
	}
}
