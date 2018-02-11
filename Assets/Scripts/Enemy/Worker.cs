using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : Enemy {

	[Header("Worker Shitting Stats")]
	public float shitRate = 1f;
	public float shitLoopTime = 10f;
	public float shitDropChance = 35f;
	public float shitSpawnRate = 30f;

	[Header("Shit Properties")]
	public float shitPushSpeed = 1f;
	public float shitSpawnForce = 3f;

	public Transform shitSpawn;
	private float nextPossibleShitTime;
	private bool isShitting = false;

	// Animations
	private Animator animator;

	public Rigidbody[] shits;

	// Use this for initialization
	public override void Start () {
		base.Start ();

		// Components
		animator = GetComponent<Animator>();

		nextPossibleShitTime = 60 / shitRate;
		StartCoroutine (SpawnShit ());
	}
	
	// Update is called once per frame
	void Update () {
		CheckForShit ();
	}

	void CheckForShit() {
		if (Time.time > nextPossibleShitTime && !isShitting) {
			//StartCoroutine (SpawnShit ());
		}
	}

	IEnumerator SpawnShit() {
		isShitting = true;
		animator.SetTrigger ("ShitStart");

		// Wait for shit start animation to finish
		yield return new WaitForSeconds (2);

		float endTime = Time.time + shitLoopTime;
		bool finished = false;
		bool shitQueued = false;
		// Try spawn shit for loop amount time
		while (!finished) {

			// RNG
			float rng = Random.Range (0, 100);
			if (rng <= shitDropChance && !shitQueued) {
				int shitIndex = Random.Range (0, shits.Length);
				Rigidbody clone = Instantiate (shits [shitIndex], shitSpawn.position, shitSpawn.rotation) as Rigidbody;
				clone.isKinematic = true;
				shitQueued = true;
				float t = shitPushSpeed / (2 * .1f);
				while (t > 0) {
					t -= Time.deltaTime;
					clone.MovePosition (clone.transform.position + (clone.transform.forward * shitPushSpeed) * Time.deltaTime);
					yield return null;
				}

				// Launch Shit
				clone.isKinematic = false;
				shitQueued = false;

				clone.AddForce (shitSpawn.transform.forward * shitSpawnForce, ForceMode.Impulse);
				clone.AddTorque (Random.insideUnitSphere * shitSpawnForce, ForceMode.Impulse);
			}
			// Check for exit
			if (Time.time > endTime) {
				finished = true;
			}
			yield return new WaitForSeconds (60 / shitSpawnRate);
		}

		// End shit
		animator.SetTrigger ("ShitEnd");
		nextPossibleShitTime = Time.time + (60 / shitRate);
		isShitting = false;
	}
}
