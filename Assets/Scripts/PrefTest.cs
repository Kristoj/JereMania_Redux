using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefTest : MonoBehaviour {

	public int iterationCount = 5000;

	// Use this for initialization
	void Start () {
		StartCoroutine (Delay());
	}

	IEnumerator Delay() {
		yield return new WaitForSeconds (3);
		PerformTest ();
	}

	void PerformTest() {
		float startTime = Time.realtimeSinceStartup;
		for (int i = 0; i < iterationCount; i++) {
			float f = Mathf.Sqrt (123456);
		}

		Debug.Log ((Time.realtimeSinceStartup - startTime).ToString("F3"));
	}
}
