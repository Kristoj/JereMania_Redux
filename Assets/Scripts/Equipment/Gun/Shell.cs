using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {

	public float lifeTime = 5f;
	public float forceMin = 70;
	public float forceMax = 150;

	private Rigidbody rig;

	// Use this for initialization
	void Start () {
		rig = GetComponent<Rigidbody> ();

		float force = Random.Range (forceMin, forceMax);
		rig.AddForce (transform.forward * force);
		rig.AddTorque (Random.insideUnitSphere * force);
	}
	
	// Update is called once per frame
	void Update () {
		lifeTime -= Time.deltaTime;

		if (lifeTime <= 0) {
			KillShell ();
		}
	}

	void KillShell() {
		Destroy (gameObject);
	}
}
