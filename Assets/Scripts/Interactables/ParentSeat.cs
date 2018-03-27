using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ParentSeat : ParentEntity {

	private ChildSeat childSeat;

	public override void Start() {
		base.Start ();
		foreach (ChildEntity ce in childEntities) {
			if (ce as ChildSeat != null) {
				childSeat = childSeat as ChildSeat;
			}
		}
	}

	public void SeatEnterRequest() {
		RpcEnterSeat ();
	}

	[ClientRpc]
	void RpcEnterSeat() {
		childSeat.EnterSeat ();
	}

	public void SeatExitRequest() {
		childSeat.ExitSeat ();
	}

	[ClientRpc]
	void RpcExitSeat() {
		childSeat.ExitSeat ();
	}
}
