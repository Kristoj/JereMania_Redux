﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Furnace : Fireplace {

	// Temperature gauge
	public Transform temperatureGaugePointer;
	public float temperatureGaugeMaxEuler = 240f;
	private Vector3 temperatureGaugeOriginalEuler;
	private float gaugeTargetEuler;

	// 
	private ChildDoor furnaceDoor;
	private FurnaceCrucibleHolder crucibleHolder;
	private Coroutine temperatureUpdateCoroutine;
	private Coroutine clientGaugeCoroutine;
	private Coroutine serverGaugeCoroutine;

	public override void Start() {
		base.Start ();

		// Store references for the child interactables
		foreach (ChildEntity ce in childEntities) {
			if (ce.name== "Furnace_Door") {
				furnaceDoor = ce as ChildDoor;
			}

			if (ce.GetType().Name == "FurnaceCrucibleHolder") {
				crucibleHolder = ce as FurnaceCrucibleHolder;
			}
		}

		if (temperatureGaugePointer != null) {
			temperatureGaugeOriginalEuler = temperatureGaugePointer.transform.localEulerAngles;
		}
	}


	public override void OnServerIgnite() {
		base.OnServerIgnite ();

		// When we ignite the furnace start heating up every crucible in the slots
		for (int i = 0; i < crucibleHolder.crucibleSlots.Length; i++) {
			if (crucibleHolder.crucibleSlots [i].crucible != null) {
				crucibleHolder.crucibleSlots [i].crucible.StartMelting (this);
			}
		}

		// Start servers gauge euler update coroutine
		if (serverGaugeCoroutine != null) {
			StopCoroutine (serverGaugeCoroutine);
		}
		serverGaugeCoroutine = StartCoroutine (ServerGaugeUpdate ());
	}

	// Server updates temperature gauges euler ever x seconds
	IEnumerator ServerGaugeUpdate() {
		while (temperature > 0 || isBurning) {
			yield return new WaitForSeconds (1f);
			RpcUpdateTemperatureGauge (temperatureGaugeOriginalEuler.z + (temperatureGaugeMaxEuler * (temperature / maxTemperature)));
		}
	}

	// Clients server gauge update coroutine start
	[ClientRpc]
	void RpcUpdateTemperatureGauge(float newEuler) {
		if (clientGaugeCoroutine != null) {
			StopCoroutine (clientGaugeCoroutine);
		}
		clientGaugeCoroutine = StartCoroutine (ClientUpdateTemperatureGauge());
		gaugeTargetEuler = newEuler;
	}

	// Update clients temperature gauge euler while the fireplace is active
	IEnumerator ClientUpdateTemperatureGauge() {
		// Lerp towards target euler
		while (true) {
			temperatureGaugePointer.transform.localEulerAngles = new Vector3 (temperatureGaugeOriginalEuler.x, temperatureGaugeOriginalEuler.y, 
				Mathf.LerpAngle (temperatureGaugePointer.transform.localEulerAngles.z, gaugeTargetEuler, .2f));
			yield return null;
		}
	}

	// 
	public override void OnClientFireplaceDeactivate() {
		if (clientGaugeCoroutine != null) {
			StopCoroutine (clientGaugeCoroutine);
		}
	}

	// Update temperature and tmeperature gauge eulers
	public override void OnTemperatureUpdate() {	
		base.OnTemperatureUpdate ();
		gaugeTargetEuler =  temperatureGaugeOriginalEuler.z + (temperatureGaugeMaxEuler * (temperature / maxTemperature));
	}

	// Child entities communicate with their parent entities using signals \\
	#region CHILD SIGNALS

	// DOOR SWING
	#region DOOR SWING
	public void SignalDoorSwing(string ciName) {

		// Determine what door we want to open and if we can open it
		int swingDir = 0;

		if (furnaceDoor == null || crucibleHolder == null) {
			return;
		}

		// Furnace door swing
		if (ciName == "Furnace_Door" && crucibleHolder.isClosed) {
			furnaceDoor.isClosed = !furnaceDoor.isClosed;

			// Close
			if (furnaceDoor.isClosed) {
				swingDir = -1;
				temperatureDescendMultiplier = 1f;
			} 

			// Open
			else {
				swingDir = 1;
				temperatureDescendMultiplier = 5.5f;
			}
		} 
		// Crucible door swing
		else if (ciName == "Crucible_Holder" && !furnaceDoor.isClosed) {
			crucibleHolder.isClosed = !crucibleHolder.isClosed;

			// Close
			if (crucibleHolder.isClosed) {
				swingDir = -1;
				// Start adding temperature to the crucible
				for (int i = 0; i < crucibleHolder.crucibleSlots.Length; i++) {
					if (crucibleHolder.crucibleSlots [i].crucible != null) {
						crucibleHolder.crucibleSlots [i].crucible.StartMelting (this);
					}
				}
			} 
			// Open
			else {
				swingDir = 1;
				// Stop adding temperature to the crucible
				for (int i = 0; i < crucibleHolder.crucibleSlots.Length; i++) {
					if (crucibleHolder.crucibleSlots [i].crucible != null) {
						crucibleHolder.crucibleSlots [i].crucible.StopMelting ();
					}
				}
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
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<PlayerWeaponController> ().ParentCurrentEquipmentToChildEntity (transform.name, entityGroup, crucibleHolder.transform.name, true);
		}
		crucibleHolder.OnClientAddCrucible (crucibleName, entityGroup);
	}
	#endregion
	#endregion
}