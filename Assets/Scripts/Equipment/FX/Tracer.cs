using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tracer : NetworkBehaviour {

	private float speed;
	private float distance;
	private Vector3 hitPoint;
	public bool useLineRenderer = true;

	IEnumerator PerformMovement() {
		float t = distance / speed;

		while (t > 0) {
			t -= Time.deltaTime;
			transform.position += transform.forward * speed * Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds (2);
		KillTracer ();
	}

	IEnumerator DrawLineRenderer() {
		float t = 3f;
		LineRenderer line = GetComponent<LineRenderer> ();

		if (line == null) {
			KillTracer();
		}
		line.SetPosition (0, transform.position);
		line.SetPosition (1, hitPoint);

		yield return new WaitForSeconds (1);

		while (t > 0) {
			t -= Time.deltaTime;
			line.material.SetColor ("_TintColor", Color.Lerp (line.material.GetColor ("_TintColor"), Color.clear, 1.2f * Time.deltaTime));
			yield return null;
		}
		KillTracer ();
	}

	void KillTracer() {
		Destroy (gameObject);
	}

	
	public void SetupTracer(Vector3 point, float newSpeed) {
		hitPoint = point;
		if (!useLineRenderer) {
			speed = newSpeed;
			distance = Vector3.Distance (transform.position, hitPoint);
			StartCoroutine (PerformMovement ());
		} else {
			StartCoroutine (DrawLineRenderer ());
		}
	}
}
