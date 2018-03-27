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
	protected float temperatureDescendMultiplier = 1f;
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

	
	/// <summary>
	/// Burn loop coroutine. This loop is active until fuel runs out or fire is extinguished.
	/// </summary>
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

	// Calls OnTemperatureUpdate constantly, while temperature is above or fireplace is burning... When we exit the loop OnServerFirePlaceDeactivate is called.
	IEnumerator UpdateTemperature() {
		while (temperature > 0 || isBurning) {
			OnTemperatureUpdate ();
			yield return null;
		}
		OnServerFireplaceDeactivate ();
	}

	// Server fireplace deactivate.. Is called when fireplace deactivates
	public virtual void OnServerFireplaceDeactivate() {
		RpcFireplaceDeactivate ();
	}


	[ClientRpc]
	void RpcFireplaceDeactivate() {
		OnClientFireplaceDeactivate ();
	}

	// Client fireplace deactivate
	public virtual void OnClientFireplaceDeactivate() {
	}

	/// <summary>
	/// Updates tempererature constantly until the fireplace is deactivated.
	/// </summary>
	public virtual void OnTemperatureUpdate() {
		temperature -= temperatureDescendRate * temperatureDescendEfficiency * temperatureDescendMultiplier * Time.deltaTime;
		temperatureDescendEfficiency += temperatureDescendEfficiencyRate / Time.deltaTime / 100;

		// Clamp values
		temperatureDescendEfficiency = Mathf.Clamp (temperatureDescendEfficiency, 0, 1);
		temperature = Mathf.Clamp (temperature, 0, maxTemperature);
	}

	/// <summary>
	/// Adds the fuel to the fireplace. Called when a player adds fuel to the fireplace.
	/// </summary>
	/// <param name="addAmount">Fuel add amount.</param>
	public virtual void AddFuel (float addAmount) {
		fuel += addAmount;
		fuel = Mathf.Clamp (fuel, 0, maxFuel);

		// Audio
		AudioManager.instance.RpcPlayCustomSound ("Place_Item", transform.position, "", 1, true);
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

	/// <summary>
	/// Extinguish the fireplace. Called when player extinguish the fireplace or fuel runs out.
	/// </summary>
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
