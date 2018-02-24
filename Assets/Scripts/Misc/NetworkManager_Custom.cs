using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManager_Custom : NetworkManager {

	public bool fastHost = true;
	public Button startHostButton;
	public Button joinGameButton;
	public GameObject inputField;
	public Text inputText;
	public Camera sceneCamera;

	public void StartupHost() {
		SetPort ();
		ToggleLobbyUI (false);
		NetworkManager.singleton.StartHost ();

		// Disable unwanted player objects
		GameObject[] playersInScene = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject p in playersInScene) {
			p.SetActive (false);
		}
	}
		

	void Start() {
		if (fastHost) {
			StartCoroutine (FastHost ());
		}
			
		if (sceneCamera != null) {
			sceneCamera.gameObject.SetActive (true);
		}

		ToggleLobbyUI (true);


	}

	public void JoinGame() {
		SetIpAddress ();
		SetPort ();
		ToggleLobbyUI (false);
		NetworkManager.singleton.StartClient ();
	}

	void SetIpAddress() {
		if (inputText.text == "") {
			NetworkManager.singleton.networkAddress = "192.168.10.47";
			return;
		}
		string ipAddress = inputText.text;
		NetworkManager.singleton.networkAddress = ipAddress;
	}

	void SetPort() {
		NetworkManager.singleton.networkPort = 7777;
		NetworkManager.singleton.maxConnections = 4;
	}

	/**
	void OnLevelWasLoaded (int level) {
		if (level == 0) {
			StartCoroutine(SetupMenuSceneButtons ());
		} else {
			SetupOtherSceneButtons ();
		}
	}
	**/

	IEnumerator SetupMenuSceneButtons() {
		yield return new WaitForSeconds (.3f);
		startHostButton.onClick.RemoveAllListeners ();
		startHostButton.onClick.AddListener (StartupHost);

		joinGameButton.onClick.RemoveAllListeners ();
		joinGameButton.onClick.AddListener (StartupHost);
	}

	void SetupOtherSceneButtons() {

	}

	IEnumerator FastHost() {
		yield return new WaitForSeconds (.15f);
		StartupHost ();
		ToggleLobbyUI (false);
	}

	void ToggleLobbyUI(bool state) {
		if (state && startHostButton != null & joinGameButton != null && inputText != null && inputField != null) {
			startHostButton.gameObject.SetActive (true);
			joinGameButton.gameObject.SetActive (true);
			inputText.gameObject.SetActive (true);
			inputField.gameObject.SetActive (true);
		} 
		else if (!state && startHostButton != null & joinGameButton != null && inputText != null && inputField != null) {
			startHostButton.gameObject.SetActive (false);
			joinGameButton.gameObject.SetActive (false);
			inputText.gameObject.SetActive (false);
			inputField.gameObject.SetActive (false);
		}
	}
}

