using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTimer : MonoBehaviour {

	public float lifeTime = 5f;

	// Use this for initialization
	void Start () {
		StartCoroutine (StartTimer ());
	}
	
	IEnumerator StartTimer() {
		yield return new WaitForSeconds (lifeTime);
		Destroy (gameObject);
	}
}
