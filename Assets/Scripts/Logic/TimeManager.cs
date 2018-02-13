using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TimeManager : NetworkBehaviour {

	public float dayLength = 360f;
	public float startTime = 140f;
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

		float t = 3;
		Debug.Log ("sleep");
		while (playersSleeping > 0 && t >= 0) {
			Debug.Log (GameManager.instance.GetPlayerCount ()-1);
			if (playersSleeping >= GameManager.instance.GetPlayerCount()-1) {
				t -= Time.deltaTime;
			}
			yield return null;
		}
		timeOfDay = dayLength / 10;
		playersSleeping = 0;
	}

	public void OnPlayerStopSleeping() {
		playersSleeping--;
		playersSleeping = Mathf.Clamp (playersSleeping, 0, GameManager.instance.GetPlayerCount ());

		// Add boons to players
		LivingEntity[] players = GameManager.GetAllPlayers();
		foreach (LivingEntity l in players) {

		}
	}

	public float GetDayPercentage() {
		return timeOfDay / dayLength;
	}
}
