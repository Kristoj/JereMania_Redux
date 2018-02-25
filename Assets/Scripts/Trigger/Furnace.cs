using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Furnace : Fireplace {

	private ChildDoor furnaceDoor;
	private ChildDoor crucibleDoor;
	private FurnaceCrucibleHolder crucibleHolder;
	private Coroutine temperatureUpdateCoroutine;

	public override void Start() {
		base.Start ();

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

	// Child entities communicate with their parent entities using signals \\
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

			// Close
			if (crucibleDoor.isClosed) {
				swingDir = -1;
				// Start coroutine that updates furnace temperature to all crucibles inside
				if (temperatureUpdateCoroutine != null) {
					StopCoroutine (temperatureUpdateCoroutine);
				}
				temperatureUpdateCoroutine = StartCoroutine (TemperatureUpdate());
			} 
			// Open
			else {
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
		if (crucibleHolder != null) {
			CmdSignalCrucibleAdd (playerName);
		}
	}

	[Command]
	void CmdSignalCrucibleAdd(string playerName) {
		if (crucibleHolder.GetEmptySlot() != null) {

			// Get empty crucible slot and get its spawn position
			FurnaceCrucibleHolder.CrucibleSlot emptySlot = crucibleHolder.GetEmptySlot();
			Vector3 spawnPos = crucibleHolder.GetSlotPos (emptySlot);
			emptySlot.hasCrucible = true;

			// Spawn new crucible
			Crucible clone = Instantiate (EquipmentLibrary.instance.GetEquipment ("Crucible"), spawnPos, Quaternion.identity) as Crucible;
			NetworkServer.Spawn (clone.gameObject);
			clone.RpcSetFurnaceMode ();

			// Subscribe to crucibles death event so CrucibleSlot will update its properties when crucible is removed
			clone.deathEvent += emptySlot.OnCrucibleRemove;

			// Signal that new crucible is spawned on the clients
			RpcSignalCrucibleAdd (playerName, clone.name, clone.entityGroupIndex);
		}
	}

	IEnumerator TemperatureUpdate() {
		while (temperature > 0) {
			for (int i = 0; i < crucibleHolder.crucibleSlots.Length; i++) {
				if (crucibleHolder.crucibleSlots [i].crucible != null) {
					crucibleHolder.crucibleSlots [i].crucible.UpdateFurnaceTemperature (0);
				}
			}
			yield return null;
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