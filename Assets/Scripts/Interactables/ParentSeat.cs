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
				childSeat = ce.GetComponent<ChildSeat>();
			}
		}
	}

	// Request seat enter
	public void SeatEnterRequest(string sourcePlayer) {
		if (childSeat.isAvailable) {
			RpcEnterSeat (sourcePlayer);
			childSeat.isAvailable = false;
		}
	}

	[ClientRpc]
	// RPC Seat enter
	void RpcEnterSeat(string sourcePlayer) {
		childSeat.EnterSeat (sourcePlayer);
	}

	// Request seat exit
	public void SeatExitRequest(string sourcePlayer) {
		childSeat.ExitSeat (sourcePlayer);
		childSeat.isAvailable = true;
	}

	[ClientRpc]
	// RPC Seat exit
	void RpcExitSeat(string sourcePlayer) {
		childSeat.ExitSeat (sourcePlayer);
	}

	// ----------------------------------------- COPY THIS TO A PARENT ENTITY --------------------------------------- \\
	/**
	
	**/
}
