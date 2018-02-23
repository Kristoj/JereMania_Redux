using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Furnace : ParentEntity {

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
	private ChildDoor furnaceDoor;
	private ChildDoor crucibleHolder;

	void Update() {
		if (isServer) {
			if (oreCount > 0 && !isMelting) {
				StartCoroutine (MeltOre ());
			}
		}
	}

	public override void Start() {
		base.Start ();
		StartCoroutine (UpdateUI ());

		// Store references for the child interactables
		foreach (ChildInteractable ci in childInteractables) {
			if (ci.name == "Furnace_Door") {
				furnaceDoor = ci as ChildDoor;
			}

			if (ci.name == "Crucible_Holder") {
				crucibleHolder = ci as ChildDoor;
			}
		}
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

	// SIGNALS\\ // SIGNALS \\
	// Child entities communicate with their parents using signals...
	#region CHILD SIGNALS

	// DOOR SWING
	#region DOOR SWING
	public void SignalDoorSwing(string ciName) {
		CmdSignalDoorSwing (ciName);
	}

	[Command]
	public void CmdSignalDoorSwing(string ciName) {

		// Determine what door we want to open and if we can open it
		int swingDir = 0;

		if (furnaceDoor == null || crucibleHolder == null) {
			return;
		}

		if (ciName == "Furnace_Door" && crucibleHolder.isClosed) {
			furnaceDoor.isClosed = !furnaceDoor.isClosed;
		
			if (furnaceDoor.isClosed) {
				swingDir = -1;
			} else {
				swingDir = 1;
			}
		} 
		else if (ciName == "Crucible_Holder" && !furnaceDoor.isClosed) {
			crucibleHolder.isClosed = !crucibleHolder.isClosed;

			if (crucibleHolder.isClosed) {
				swingDir = -1;
			} else {
				swingDir = 1;
			}
		}

		// Open the target door if we allowed
		if (swingDir != 0) {
			RpcSignalDoorSwing (swingDir, ciName);
		}
	}

	[ClientRpc]
	public void RpcSignalDoorSwing(int swingDir, string ciName) {
		// Open the correct door
		if (ciName == "Furnace_Door" && furnaceDoor != null) {
			furnaceDoor.SwingDoor (swingDir);
		} else if (ciName == "Crucible_Holder" && crucibleHolder != null) {
			crucibleHolder.SwingDoor (swingDir);
		}
	}

	#endregion

	// Crucible Add
	#region Curcible Add
	public void SignalCrucibleAdd() {
		CmdSignalCrucibleAdd ();
	}

	[Command]
	void CmdSignalCrucibleAdd() {

	}

	[ClientRpc]
	void RpcSignalCrucibleAdd() {

	}
	#endregion
	#endregion
}