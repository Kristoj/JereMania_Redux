using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildInteractable : ChildEntity {

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
