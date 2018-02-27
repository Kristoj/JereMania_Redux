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


	public override void OnServerIgnite() {
		base.OnServerIgnite ();

		for (int i = 0; i < crucibleHolder.crucibleSlots.Length; i++) {
			if (crucibleHolder.crucibleSlots [i].crucible != null) {
				crucibleHolder.crucibleSlots [i].crucible.StartMelting (this);
			}
		}
	}
	// Child entities communicate with their parent entities using signals \\
	#region CHILD SIGNALS

	// DOOR SWING
	#region DOOR SWING
	public void SignalDoorSwing(string ciName) {

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
	public void SignalCrucibleAdd(string playerName, string crucibleName, int crucibleGroup) {
		if (crucibleHolder != null) {
			CmdSignalCrucibleAdd (playerName, crucibleName, crucibleGroup);
		}
	}

	[Command]
	void CmdSignalCrucibleAdd(string playerName, string crucibleName, int crucibleGroup) {
		if (crucibleHolder.GetEmptySlot() != null) {

			// Get empty crucible slot and get its spawn position
			FurnaceCrucibleHolder.CrucibleSlot emptySlot = crucibleHolder.GetEmptySlot();
			Vector3 spawnPos = crucibleHolder.GetSlotPos (emptySlot);

			// Spawn new crucible
			Crucible targetCrucible = GameManager.instance.GetEntity(crucibleName, crucibleGroup) as Crucible;
			if (targetCrucible == null) {
				return;
			}

			// Remove crucible from the player who added the crucible to the furnace
			if (GameManager.GetPlayerByName (playerName).name == playerName) {
				GameManager.GetPlayerByName (playerName).GetComponent<GunController> ().EquipEquipment ("", 0, true, 0);
			}

			// Update vars of the empty slot and it's crucible
			emptySlot.crucible = targetCrucible;
			emptySlot.OnCrucibleAdd (this);
			targetCrucible.RpcSetFurnaceMode (spawnPos, transform.eulerAngles);

			// Subscribe to crucibles death event so CrucibleSlot will update its properties when crucible is removed
			targetCrucible.pickupEvent += emptySlot.OnCrucibleRemove;

			// Signal that new crucible is spawned on the clients
			RpcSignalCrucibleAdd (playerName, targetCrucible.name, targetCrucible.entityGroupIndex);
		}
	}

	[ClientRpc]
	void RpcSignalCrucibleAdd(string playerName, string crucibleName, int entityGroup) {
		crucibleHolder.AddCrucible (crucibleName, entityGroup);
	}
	#endregion
	#endregion
}