using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroSeat : ChildSeat {
	
	// On enter
	public override void OnServerStartInteraction(string sourcePlayer) {
		base.OnServerStartInteraction (sourcePlayer);
		transform.parent.GetComponent<GyroController> ().OnEnter(sourcePlayer);
	}
	// On exit
	public override void OnServerExit(string sourcePlayer) {
		base.OnServerExit (sourcePlayer);
		transform.parent.GetComponent<GyroController> ().OnExit(sourcePlayer);
	}
}
