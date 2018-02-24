using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : Seat {

	public override void OnClientStartInteraction(string masterId) {
		if (TimeManager.instance.GetDayPercentage () > .5f) {
			base.OnClientStartInteraction (masterId);
		}
	}
		
	public override void OnServerEnterSeat() {
		base.OnServerEnterSeat ();
		TimeManager.instance.OnPlayerStartSleeping ();
	}

	public override void OnServerExitSeat() {
		base.OnServerExitSeat ();
		TimeManager.instance.OnPlayerStopSleeping ();
	}
}
