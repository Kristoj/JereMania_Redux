using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fireplace : ParentEntity {

	[Header("Temperature Settings")]
	public float maxTemperature = 800f;
	public float temperature = 0;
	[Tooltip("How quickly the temperature will rise in a second")]
	public float temperatureClimbRate = 5f;
	[Tooltip("How efficiently the temperature will rise in a second. 0-100%")]
	public float temperatureClimbEfficiency = 0f;
	[Tooltip("How much temperature climb efficiency rate will increase in a second")]
	public float temperatureClimbEfficiencyRate = 10;
	[Tooltip("How quickly the temperature will descend in a second")]
	public float temperatureDescendRate = 1.5f;
	[Tooltip("How efficiently the temperature will descend in a second. 0-100%")]
	public float temperatureDescendEfficiency = 0f;
	[Tooltip("How efficiently the temperature will rise in a second. 0-100%")]
	public float temperatureDescendEfficiencyRate = 5f;
	[Tooltip("How efficiently temperature is added to objects inside the fireplace per second. 0-100%")]
	public float temperatureEfficiency = 1;

	[Header ("Fuel Settings")]
	public float maxFuel = 100f;
	[Tooltip("How much fuel is consumed per second")]
	public float fuelConsumptionRate = .5f;
	public float fuel = 0;
	public bool isBurning = false;

	private Coroutine burnCoroutine;
	private Coroutine coolingCoroutine;

	void start() {
		fuel = maxFuel;
	}

	IEnumerator Burn() {

		// Set temperature vars
		temperatureClimbEfficiency = 0;
		temperatureDescendEfficiency = 0;
		isBurning = true;

		while (fuel > 0) {
			fuel -= fuelConsumptionRate * Time.deltaTime;
			fuel = Mathf.Clamp (fuel, 0, maxFuel);

			// Rise temperature
			temperature += temperatureClimbRate * temperatureClimbEfficiency * Time.deltaTime;
			temperatureClimbEfficiency += temperatureClimbEfficiencyRate * Time.deltaTime / 100;

			// Clamp values
			temperatureClimbEfficiency = Mathf.Clamp (temperatureClimbEfficiency, 0, 1);
			temperature = Mathf.Clamp (temperature, 0, maxTemperature);
			yield return null;
		}
		Extinguish ();
	}

	IEnumerator Cooling() {
		while (temperature > 0 || isBurning) {
			temperature -= temperatureDescendRate * temperatureDescendEfficiency * Time.deltaTime;
			temperatureDescendEfficiency += temperatureDescendEfficiencyRate / Time.deltaTime / 100;

			// Clamp values
			temperatureDescendEfficiency = Mathf.Clamp (temperatureDescendEfficiency, 0, 1);
			temperature = Mathf.Clamp (temperature, 0, maxTemperature);
			yield return null;
		}
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

	public virtual void Ignite() {
		// Start burning
		if (burnCoroutine != null) {
			StopCoroutine (burnCoroutine);
		}
		burnCoroutine = StartCoroutine (Burn ());
		// Start cooling
		if (coolingCoroutine != null) {
			StopCoroutine (coolingCoroutine);
		}
		coolingCoroutine = StartCoroutine (Cooling());
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
		if (burnCoroutine != null) {
			StopCoroutine (burnCoroutine);
		}
	}
}
