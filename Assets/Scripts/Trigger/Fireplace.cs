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
	[Tooltip("How much temperature climb efficiency will increase in a second")]
	public float temperatureClimbEfficiencyRate = 10;
	[Tooltip("How quickly the temperature will descend in a second")]
	public float temperatureDescendRate = 1.5f;
	[Tooltip("How efficiently the temperature will descend in a second. 0-100%")]
	public float temperatureDescendEfficiency = 0f;
	[Tooltip("How efficiently the descend efficiency will descend in a second. 0-100%")]
	public float temperatureDescendEfficiencyRate = 5f;
	[Tooltip("How efficiently temperature is added to objects inside the fireplace per second. 0-100%")]
	public float temperatureEfficiency = 1;

	[Header ("Fuel Settings")]
	public float maxFuel = 100f;
	[Tooltip("How much fuel is consumed per second")]
	public float fuelConsumptionRate = .5f;
	public float fuel = 0;
	public bool isBurning = false;

	[Header ("Effects")]
	public ParticleSystem fireParticle;
	protected ParticleSystem emberParticle;

	private Coroutine burnCoroutine;
	private Coroutine tempUpdateCoroutine;

	void start() {
		fuel = maxFuel;

		// Get particle references
		if (fireParticle != null) {
			for (int i = 0; i < fireParticle.transform.childCount; i++) {
				if (fireParticle.transform.GetChild (i).name == "FireEmbers") {
					emberParticle = fireParticle.transform.GetChild (i).GetComponent<ParticleSystem> ();
				}
			}
		}
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

	IEnumerator UpdateTemperature() {
		while (temperature > 0 || isBurning) {
			OnTemperatureUpdate ();
			yield return null;
		}
		OnServerFireplaceDeactivate ();
	}

	public virtual void OnServerFireplaceDeactivate() {
		RpcFireplaceDeactivate ();
	}

	[ClientRpc]
	void RpcFireplaceDeactivate() {
		OnClientFireplaceDeactivate ();
	}

	public virtual void OnClientFireplaceDeactivate() {
	}

	public virtual void OnTemperatureUpdate() {
		temperature -= temperatureDescendRate * temperatureDescendEfficiency * Time.deltaTime;
		temperatureDescendEfficiency += temperatureDescendEfficiencyRate / Time.deltaTime / 100;

		// Clamp values
		temperatureDescendEfficiency = Mathf.Clamp (temperatureDescendEfficiency, 0, 1);
		temperature = Mathf.Clamp (temperature, 0, maxTemperature);
	}

	public virtual void AddFuel (string playerName, string entityName) {
		if (entityName == "Wood") {
			fuel += 20;
			RpcAddFuel (playerName);
		}

		// Audio
		AudioManager.instance.RpcPlayCustomSound ("Place_Item", transform.position, "", 1, true);
	}

	[ClientRpc]
	void RpcAddFuel(string playerName) {
		if (GameManager.GetLocalPlayer ().name == playerName) {
			GameManager.GetLocalPlayer ().GetComponent<GunController> ().DestroyCurrentEquipment (true);
		}
	}

	#region Ignite
	void Ignite() {
		OnServerIgnite ();
	}

	public virtual void OnServerIgnite() {
		// Start burning
		if (burnCoroutine != null) {
			StopCoroutine (burnCoroutine);
		}
		burnCoroutine = StartCoroutine (Burn ());
		// Start cooling
		if (tempUpdateCoroutine != null) {
			StopCoroutine (tempUpdateCoroutine);
		}
		tempUpdateCoroutine = StartCoroutine (UpdateTemperature());
		// Update fire effects for all clients
		RpcIgnite ();
	}

	[ClientRpc]
	public virtual void RpcIgnite() {
		// FX
		// Set new vars for the particles
		ParticleSystem.EmissionModule fireEm = fireParticle.emission;
		ParticleSystem.EmissionModule emberEm = fireParticle.emission;
		fireEm.rateOverTime = 5;
		emberEm.rateOverTime = 5;
		// Play particles
		fireParticle.Play();
	}
	#endregion

	public virtual void Extinguish() {
		isBurning = false;
		if (burnCoroutine != null) {
			StopCoroutine (burnCoroutine);
		}
		// Update fire effects for all clients
		RpcExtinguish ();
	}

	[ClientRpc]
	protected void RpcExtinguish() {
		// FX
		// Set new vars for the particles
		ParticleSystem.EmissionModule fireEm = fireParticle.emission;
		ParticleSystem.EmissionModule emberEm = fireParticle.emission;
		fireEm.rateOverTime = 0;
		emberEm.rateOverTime = 0;
		// Stop particles
		fireParticle.Stop();
	}

	// Called when player presses 'Use' key down when aiming at the trigger... This method is called on the server
	public void ToggleFire() {
		if (isBurning) {
			Extinguish ();
		} else {
			if (fuel > 0) {
				Ignite ();
			}
		}
	}
}
