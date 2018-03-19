using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {

	public Behaviour[] componentsToDisable;
	public SkinnedMeshRenderer[] viewMeshesToDisable;
	public GameObject[] serverMeshesToDisable;
	private Camera sceneCamera;

	void Start() {
		// If we're NOT controlling the player
		if (!isLocalPlayer) {
			for (int i = 0; i < serverMeshesToDisable.Length; i++) {
				if (serverMeshesToDisable [i] != null) {
					serverMeshesToDisable [i].SetActive (false);
				}
			}
			for (int i = 0; i < componentsToDisable.Length; i++) {
				if (componentsToDisable [i] != null) {
					componentsToDisable [i].enabled = false;
				}
			}
		} 
		// If we're controlling the player
		else {
			sceneCamera = Camera.main;
			if (sceneCamera != null) {
				sceneCamera.gameObject.SetActive (false);
			}
			for (int i = 0; i < viewMeshesToDisable.Length; i++) {
				if (viewMeshesToDisable [i] != null) {
					viewMeshesToDisable [i].enabled = false;
				}
			}
			transform.tag = "LocalPlayer";
		}
	}

	public override void OnStartClient() {
		base.OnStartClient ();
	}

	void OnDisable() {
		if (sceneCamera != null) {
			sceneCamera.gameObject.SetActive (true);
		}
	}

}
