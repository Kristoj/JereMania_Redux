using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TimeManager : NetworkBehaviour {

	public float dayLength = 360f;
	public float startTime = 140f;
	[SyncVar]
	public float timeOfDay;
	public float curDay;

	[Header("Lighting")]
	public Light sun;
	public AnimationCurve lightCurve;
	private float ogAmbientIntensity;

	// Use this for initialization
	void Start () {
		timeOfDay = startTime;
		ogAmbientIntensity = RenderSettings.ambientIntensity;
	}
	
	// Update is called once per frame
	void Update () {
		if (isServer) {
			UpdateTime ();
		}

		if (sun != null) {
			RotateSun ();
		}
	}

	void UpdateTime() {
		timeOfDay += Time.deltaTime;

		if (timeOfDay >= dayLength) {
			curDay++;
			timeOfDay = 0;
		}
	}

	void RotateSun() {
		float cyclePercentage = timeOfDay / dayLength;

		Vector3 sunRot = new Vector3 (360 * cyclePercentage, sun.transform.rotation.y, sun.transform.rotation.z);
		sun.transform.rotation = Quaternion.Euler (sunRot);

		// Lighting
		sun.intensity = lightCurve.Evaluate (cyclePercentage);
		RenderSettings.ambientIntensity = ogAmbientIntensity * lightCurve.Evaluate (cyclePercentage);
	}
}
