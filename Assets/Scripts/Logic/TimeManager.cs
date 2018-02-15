using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TimeManager : NetworkBehaviour {

	public float dayLength = 360f;
	public float startTime = 80;
	public float sunRotationUpdateRate = 0f;
	[SyncVar]
	public float timeOfDay;
	public float curDay;
	public bool canRotate = true;

	[Header("Lighting")]
	public Light sun;
	public AnimationCurve lightCurve;
	private float ogAmbientIntensity;
	private int playersSleeping;
	public static TimeManager instance;

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		timeOfDay = startTime;
		ogAmbientIntensity = RenderSettings.ambientIntensity;
		if (sun != null) {
			StartCoroutine (UpdateSunRotation ());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isServer) {
			UpdateTime ();
		}
	}

	void UpdateTime() {
		timeOfDay += Time.deltaTime;

		if (timeOfDay >= dayLength) {
			curDay++;
			timeOfDay = 0;
		}
	}

	IEnumerator UpdateSunRotation() {
		while (canRotate) {
			float cyclePercentage = timeOfDay / dayLength;

			Vector3 sunRot = new Vector3 (360 * cyclePercentage, sun.transform.rotation.y, sun.transform.rotation.z);
			sun.transform.rotation = Quaternion.Euler (sunRot);
			// Lighting
			sun.intensity = lightCurve.Evaluate (cyclePercentage);
			RenderSettings.ambientIntensity = ogAmbientIntensity * lightCurve.Evaluate (cyclePercentage);
			yield return new WaitForSeconds (sunRotationUpdateRate);
		}
	}

	public void OnPlayerStartSleeping() {
		playersSleeping++;
		StartCoroutine (SleepCycle ());
	}

	IEnumerator SleepCycle() {

		float t = 5;
		while (playersSleeping > 0 && t >= 0) {
			if (playersSleeping >= GameManager.instance.GetPlayerCount()-1) {
				t -= Time.deltaTime;
			}
			yield return null;
		}
		timeOfDay = startTime;
		playersSleeping = 0;

		RpcOnPlayerWakeUp ();
	}

	[ClientRpc]
	void RpcOnPlayerWakeUp() {
		// Add boons to players
		Player localPlayer = GameManager.GetLocalPlayer();

		if (localPlayer != null) {
			PlayerStats playerStats = localPlayer.GetComponent<PlayerStats> ();
			playerStats.FatiqueAdd (100);
			playerStats.StaminaAdd (100);
		}
	}

	public void OnPlayerStopSleeping() {
		playersSleeping--;
		playersSleeping = Mathf.Clamp (playersSleeping, 0, GameManager.instance.GetPlayerCount ());
	}

	public float GetDayPercentage() {
		return timeOfDay / dayLength;
	}
}
