using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Furnace : Entity {

	public int maxOreCount = 5;
	public float meltTime = 11f;
	public float meltSuccesChance = 18;
	public float fuelConsumption = .75f;
	public Rigidbody dropResource;
	public Text fuelText;
	private int oreCount = 0;
	[SyncVar]
	private float fuel = 100f;
	private bool isMelting = false;
	[HideInInspector]
	public bool canUpdateUI = true;
	private List<ChildInteractable> childInteractables = new List<ChildInteractable> ();

	void Update() {
		if (isServer) {
			if (oreCount > 0 && !isMelting) {
				StartCoroutine (MeltOre ());
			}
		}
	}

	void Start() {
		StartCoroutine (UpdateUI ());
	}

	void OnTriggerEnter (Collider c) {

		if (isServer) {
			Resource dynamicEntity = c.GetComponent<Resource> ();

			if (dynamicEntity != null && dynamicEntity.isAvailable) {
				if (dynamicEntity.entityName == "Ore" && oreCount < maxOreCount) {
					oreCount++;
					NetworkServer.Destroy (c.gameObject);
				}

				if (dynamicEntity.entityName == "Wood") {
					fuel += 20;
					fuel = Mathf.Clamp (fuel, 0, 100);
					NetworkServer.Destroy (c.gameObject);
				}
			}
		}
	}

	IEnumerator MeltOre() {
		float t = 0;
		isMelting = true;
		while (t < meltTime) {
			if (fuel > 0) {
				t += Time.deltaTime;
				fuel -= Time.deltaTime * fuelConsumption;
			}
			yield return null;
		}

		// Spawn mineral
		int rng = Random.Range (0, 100);

		if (rng < meltSuccesChance) {
			Rigidbody clone = Instantiate (dropResource, transform.position + transform.up, transform.rotation);
			NetworkServer.Spawn (clone.gameObject);
			RpcSpawnMineral (clone.GetComponent<NetworkIdentity>().netId);
		}
		oreCount--;
		isMelting = false;
	}

	[ClientRpc]
	void RpcSpawnMineral(NetworkInstanceId cloneId) {
		GameObject cloneObject = ClientScene.FindLocalObject (cloneId);
		if (cloneObject != null) {
			Rigidbody rg = cloneObject.GetComponent<Rigidbody> ();
			rg.AddForce (Vector3.up * 5 * rg.mass, ForceMode.Impulse);
			rg.AddTorque (Random.insideUnitSphere * 5 * rg.mass, ForceMode.Impulse);
		}
	}

	IEnumerator UpdateUI() {
		while (canUpdateUI && fuelText != null) {
			fuelText.text = fuel.ToString("F0") + "%";
			yield return null;
		}
		yield return null;
	}

	// Called when client signals that he wants to open a door
	public void SignalDoorOpen(string childName) {
		// Send signal to server so we can client RPC door opening
		CmdSignalDoorOpen (childName);
	}
		
	[Command]
	public void CmdSignalDoorOpen(string childName) {
		// Send signal to every client that player wants to open a door
		RpcSignalDoorOpen (childName);
	}

	[ClientRpc]
	public void RpcSignalDoorOpen(string childName) {
		// Open the door in the door class
		foreach (ChildInteractable ci in childInteractables) {
			if (ci.name == childName) {
				ci.SendMessage ("OpenDoor", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void RegisterChildInteractable(ChildInteractable ci) {
		if (!childInteractables.Contains(ci)) {
			childInteractables.Add (ci);
		}
	}
}
