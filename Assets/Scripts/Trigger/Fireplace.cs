using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fireplace : ParentEntity {

	[Header("Temperature Settings")]
	public float temperature = 0;
	[Tooltip("How quickly the temperature will rise in a second")]
	public float temperatureClimbRate = 5f;
	[Tooltip("How efficiently the temperature will rise in a second. 0-100%")]
	public float temperatureClimbEfficiency = 0f;
	[Tooltip("How quickly the temperature will descend in a second")]
	public float temperatureDescendRate = 1.5f;
	[Tooltip("How efficiently the temperature will descend in a second. 0-100%")]
	public float temperatureDescendEfficiency = 0f;
	[Tooltip("How efficiently temperature is added to objects inside the fireplace per second. 0-100%")]
	public float temperatureEfficiency = 1;

	[Header ("Fuel Settings")]
	public float maxFuel = 100f;
	[Tooltip("How much fuel is consumed per second")]
	public float fuelConsumptionRate = .5f;
	public float fuel = 0;
	public bool isBurning = false;

	private Coroutine burnCoroutine;

	void start() {
		fuel = maxFuel;
	}

	IEnumerator Burn() {
		isBurning = true;
		while (fuel > 0) {
			fuel -= fuelConsumptionRate * Time.deltaTime;
			fuel = Mathf.Clamp (fuel, 0, maxFuel);

			// Calculate temperature

			yield return null;
		}
		Extinguish ();
	}

	public void AddFuel (string playerName, string entityName) {
		if (entityName == "Wood") {
			fuel += 20;
			RpcAddFuel (playerName);
		}
	}

	[ClientRpc]
	void RpcAddFuel(string playerName) {
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<GunController> ().EquipEquipment (null, false, 0);
		}
	}

	public void Ignite() {
		if (burnCoroutine != null) {
			StopCoroutine (burnCoroutine);
		}
		burnCoroutine = StartCoroutine (Burn ());
	}

	public void ToggleFire() {
		if (isBurning) {
			Extinguish ();
		} else {
			if (fuel > 0) {
				Ignite ();
			}
		}
	}

	public void Extinguish() {
		isBurning = false;
	}
}
