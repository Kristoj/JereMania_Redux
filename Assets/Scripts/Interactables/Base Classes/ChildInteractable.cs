using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildInteractable : ChildEntity {

	// Vars
	public bool isAvailable = true;

	public virtual void OnClientStartInteraction (string sourcePlayer) {

	}

	public virtual void OnServerStartInteraction (string sourcePlayer) {
		
	}

	public virtual void OnClientStartPickup (string sourcePlayer) {

	}

	public virtual void OnServerStartPickup (string sourcePlayer) {

	}

	public virtual void OnServerExit(string sourcePlayer) {

	}
}
