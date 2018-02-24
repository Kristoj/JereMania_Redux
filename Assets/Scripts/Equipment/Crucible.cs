using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Crucible : Equipment {

	[ClientRpc]
	public void RpcSetFurnaceMode() {
		rig = GetComponent<Rigidbody> ();
		rig.isKinematic = true;
	}
}
