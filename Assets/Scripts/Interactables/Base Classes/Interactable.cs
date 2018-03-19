using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Interactable : Entity {

	public Transform owner;

	public override void OnStartClient() {
		base.OnStartClient ();
	}

	// Server START interaction
	public virtual void OnServerStartInteraction(string masterId) {
		OnInteractablePickup (masterId);
	}

	// Server END interaction
	public virtual void OnServerEndInteraction() {

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
	public virtual void OnClientStartSwap(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
		}
	}

	// Server pickup START
	public virtual void OnServerStartSwap(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
			OnInteractablePickup (masterId);
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
		
	public virtual void OnExit(string masterId) {
		owner = null;
	}
}
