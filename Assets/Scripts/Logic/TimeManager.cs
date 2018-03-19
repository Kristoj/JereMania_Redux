using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

	[HideInInspector]
	public float gameSecond;
	public int clockSecond;
	public int clockMinute;
	public int clockHour;

	public Text viliText;

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		timeOfDay = startTime;
		ogAmbientIntensity = RenderSettings.ambientIntensity;
		if (sun != null) {
			StartCoroutine (UpdateSunRotation ());
			StartCoroutine (ClockTime ());
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

	IEnumerator ClockTime() {
		gameSecond = dayLength / 86400;
		while(sun != null){
		yield return new WaitForSecondsRealtime (gameSecond);
		++clockSecond;
		if (clockSecond >= 60) {
			++clockMinute;
			clockSecond = 0;
		}
		if (clockMinute >= 60){
			++clockHour;
			clockMinute = 0;
		}
		if (clockHour >= 24){
			clockHour = 0;
		}
			viliText.text = (clockHour + ":" + clockMinute + ":" + clockSecond);
		}
	
	}

	[ClientRpc]
	void RpcOnPlayerWakeUp() {
		// Add boons to players
		Player localPlayer = GameManager.GetLocalPlayer();

		if (localPlayer != null) {
			PlayerStats playerStats = localPlayer.GetComponent<PlayerStats> ();
			playerStats.FatiqueAdd (100);
			playerStats.StaminaAdd (100);
			playerStats.HungerAdd (100);
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
