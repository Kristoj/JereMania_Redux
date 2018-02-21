using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Interactable : Entity {

	[HideInInspector]
	[SyncVar]
	public bool isAvailable = true;
	protected Transform owner;

	public override void OnStartClient() {
		base.OnStartClient ();
	}

	/// <summary>
	/// Called when player is aiming at a interactable object and "Interact" key is down.
	/// </summary>
	/// <param name="masterId">Players name who called this function.</param>
	// Client interaction START
	public virtual void OnClientStartInteraction(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
		}
	}

	/// <summary>
	/// Called when player has a interactable object as a target and "Interact" key is up.
	/// </summary>
	/// <param name="masterId">Players name who called this function.</param>
	// Client interaction END
	public virtual void OnClientEndInteraction(string masterId) {

	}

	// Client pickup START
	public virtual void OnStartPickup(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
		}
	}

	/// <summary>
	/// Called once when player is starts aiming at a interactable object
	/// </summary>
	// Client focus ENTER
	public virtual void OnClientFocusEnter() {

	}

	/// <summary>
	/// Called once when player is not longer aiming at a interactable object
	/// </summary>
	// Client focus EXIT
	public virtual void OnClientFocusExit() {

	}

	// Server START interaction
	public virtual void OnServerStartInteraction(string masterId) {
		
	}

	// Server END interaction
	public virtual void OnServerEndInteraction() {
		
	}
		
	public virtual void OnExit(string masterId) {
		owner = null;
	}

	public class FocusWhiteList {
		
	}
}
