using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LightSwitch : Interactable {

	public Light[] lights;
	private bool isOn;

	public override void Start() {
		//isOn = lights [0].enabled;
	}

	public override void OnStartInteraction(string masterId) {
		CmdToggleLights ();
	}

	[Command]
	void CmdToggleLights() {

		if (lights.Length <= 0) {
			return;
		}

		isOn = !isOn;
		RpcToggleLights (isOn);
	}

	[ClientRpc]
	void RpcToggleLights(bool isOn) {
		// Toggle light
		foreach (Light l in lights) {
			l.enabled = isOn;
		}
	}
}
