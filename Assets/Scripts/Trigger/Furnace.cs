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
	private ChildDoor crucibleDoor;
	private FurnaceCrucibleHolder crucibleHolder;

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
		foreach (ChildEntity ce in childEntities) {
			if (ce.name== "Furnace_Door") {
				furnaceDoor = ce as ChildDoor;
			}

			if (ce.GetType().Name == "ChildDoor" && ce.name == "Crucible_Holder") {
				crucibleDoor = ce as ChildDoor;
			}

			if (ce.GetType().Name == "FurnaceCrucibleHolder") {
				crucibleHolder = ce as FurnaceCrucibleHolder;
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
		GameManager.GetLocalPlayer().SetAuthority (netId, GameManager.GetLocalPlayer().GetComponent<NetworkIdentity>());
		CmdSignalDoorSwing (ciName);
	}

	[Command]
	public void CmdSignalDoorSwing(string ciName) {

		// Determine what door we want to open and if we can open it
		int swingDir = 0;

		if (furnaceDoor == null || crucibleDoor == null) {
			return;
		}

		if (ciName == "Furnace_Door" && crucibleDoor.isClosed) {
			furnaceDoor.isClosed = !furnaceDoor.isClosed;
		
			if (furnaceDoor.isClosed) {
				swingDir = -1;
			} else {
				swingDir = 1;
			}
		} 
		else if (ciName == "Crucible_Holder" && !furnaceDoor.isClosed) {
			crucibleDoor.isClosed = !crucibleDoor.isClosed;

			if (crucibleDoor.isClosed) {
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
		} else if (ciName == "Crucible_Holder" && crucibleDoor != null) {
			crucibleDoor.SwingDoor (swingDir);
		}
	}

	#endregion

	// Crucible Add
	#region Curcible Add
	public void SignalCrucibleAdd(string playerName) {
		// Assign authority
		GameManager.GetLocalPlayer().SetAuthority (netId, GameManager.GetLocalPlayer().GetComponent<NetworkIdentity>());
		if (crucibleHolder != null) {
			CmdSignalCrucibleAdd (playerName);
		}
	}

	[Command]
	void CmdSignalCrucibleAdd(string playerName) {
		if (crucibleHolder.crucibleCount < crucibleHolder.crucibleSlots) {
			// Spawn new crucible
			Crucible clone = Instantiate (EquipmentLibrary.instance.GetEquipment ("Crucible"), crucibleHolder.transform.position, Quaternion.identity) as Crucible;
			NetworkServer.Spawn (clone.gameObject);
			clone.RpcSetFurnaceMode ();

			// Signal that new crucible is spawned on the clients
			RpcSignalCrucibleAdd (playerName, clone.name, clone.entityGroupIndex);
		}
	}

	[ClientRpc]
	void RpcSignalCrucibleAdd(string playerName, string crucibleName, int entityGroup) {
		// Remove crucible from the player who added the crucible to the furnace
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<GunController> ().EquipEquipment (null, false, 0);
		}
		crucibleHolder.AddCrucible (crucibleName, entityGroup);
	}
	#endregion
	#endregion
}