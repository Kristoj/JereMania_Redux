using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactFX : MonoBehaviour {

	public float lifeTime = 6f;

	// Use this for initialization
	void Start () {
		StartCoroutine (LifeTimer ());
	}
	
	IEnumerator LifeTimer() {
		yield return new WaitForSeconds (lifeTime);
		KillParticle ();
	}

	void KillParticle() {
		Destroy (gameObject);
	}
}
