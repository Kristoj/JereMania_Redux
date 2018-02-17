﻿using UnityEngine;
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

	public virtual void OnStartInteraction(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
		}
	}

	public virtual void OnStartPickup(string masterId) {
		if (isAvailable) {
			owner = GameManager.GetPlayerByName (masterId).transform;
		}
	}

	public virtual void OnClientEndInteraction() {

	}

	public virtual void OnServerStartInteraction(string masterId) {
		
	}

	public virtual void OnServerEndInteraction() {
		
	}

	public  virtual void OnUse() {

	}

	public virtual void OnExit(string masterId) {
		owner = null;
	}

	public virtual void OnFocus() {
		//if (PlayerUI.instance.focusText != null) {
		//	PlayerUI.instance.focusText.text = objectName;
		//}
	}
}
