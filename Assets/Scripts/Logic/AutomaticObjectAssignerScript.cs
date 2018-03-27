using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AutomaticObjectAssignerScript : MonoBehaviour {

	private SoundLibrary soundLibrary;
	private ItemDatabase equipmentLibrary;
	private NetworkManager_Custom customNetworkManager;

	public void AssignObjects() {
		// Get reference to components
		equipmentLibrary = GetComponent<ItemDatabase> ();
		soundLibrary = GetComponent<SoundLibrary> ();
		customNetworkManager = GameObject.Find("#NETWORKMANAGER").GetComponent<NetworkManager_Custom> ();

		// Assign files
		AssignEquipment ();
		AssignAudioFiles ();
		AssignNetworkPrefabs ();
	}

	void AssignEquipment() {
		object[] equipments = Resources.LoadAll ("Prefabs/Equipment", typeof(Equipment));

		// Clear equipment list first
		equipmentLibrary.equipmentList.Clear();
		foreach (object o in equipments) {
			if (!equipmentLibrary.equipmentList.Contains (o as Equipment)) {
				equipmentLibrary.equipmentList.Add (o as Equipment);
			}
		}
	}

	void AssignAudioFiles () {
		object[] audioFiles = Resources.LoadAll ("Audio", typeof(AudioClip));

		// Clear equipment list first
		soundLibrary.sounds.Clear();
		foreach (object o in audioFiles) {
			if (!soundLibrary.sounds.Contains (o as AudioClip)) {
				soundLibrary.sounds.Add (o as AudioClip);
			}
		}
	}

	void AssignNetworkPrefabs() {
		// Create Lists and arrays
		object[] networkPrefab = Resources.LoadAll ("Prefabs", typeof(NetworkIdentity));
		object[] gameObjectPrefab = Resources.LoadAll ("Prefabs", typeof(GameObject));
		List <GameObject> gmList = new List<GameObject>();
		List <string> netList = new List<string>();

		// Setup reference lists
		foreach (object g in gameObjectPrefab) {
			gmList.Add (g as GameObject);
		}

		// Assign gameobjects that have networkidentity to a list
		List <GameObject> chosenGameObjects = new List<GameObject> ();
		foreach (object n in networkPrefab) {
			string s = n.ToString();
			s = s.Substring (0, s.Length - 41);
			netList.Add (s);

			foreach (GameObject obj in gmList) {
				if (netList.Contains (obj.name)) {
					chosenGameObjects.Add (obj);
				}
			}
		}

		// Clear equipment list first
		customNetworkManager.spawnPrefabs.Clear();
		foreach (GameObject o in chosenGameObjects) {
			if (!customNetworkManager.spawnPrefabs.Contains (o)) {
				customNetworkManager.spawnPrefabs.Add (o);
			}
		}
	}
}