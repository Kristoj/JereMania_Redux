using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitch : Interactable {

	public Door door;

	public override void OnClientStartInteraction (string masterId) {
		if (door != null) {
			door.CmdMoveDoor ();
		}
	}
}
