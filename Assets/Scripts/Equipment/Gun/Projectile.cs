using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Projectile : NetworkBehaviour {
	public float damage = 25f;
	public float moveSpeed = 10f;
	public float lifeTime = 10f;
	public LayerMask hitMask;
	[HideInInspector]
	public Vector3 velocity;
	private Vector3 targetPos;

	private void FixedUpdate() {
		// we want the bullet to be updated only on the server
		if (!base.isServer)
			return;

		// Move bullet on the server
		//targetPos = transform.position += velocity * Time.deltaTime * moveSpeed;
		if (isServer) {
			//transform.position = targetPos;
		}
	}

	public virtual void Update() {
		if (!base.isServer) {
			return;
		}

		PerformMovement();

		// Reduce lifetime
		lifeTime -= Time.deltaTime;


		if (lifeTime <= 0) {
			KillProjectile ();
		}
	}

	public virtual void PerformMovement() {
		float moveDistance = moveSpeed * Time.deltaTime * 1.5f;
		CheckCollisions (moveDistance);
	}

	public void CheckCollisions(float moveDistance) {
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, moveDistance, hitMask)) {
			//OnHitObject (hit);
		}
	}/**

	public virtual void OnHitObject(RaycastHit hit) {
		LivingEntity dable = hit.collider.GetComponent<LivingEntity> ();
		if (dable != null) {
			dable.TakeDamage (damage);
		}

		// Impact FX
		if (impactParticle != null) {
			ImpactFX clone = (ImpactFX)Instantiate(impactParticle, transform.position, Quaternion.Euler(-transform.forward));
			NetworkServer.Spawn (clone.gameObject);
		}

		KillProjectile ();
	}
	**/

	public void KillProjectile() {
		Destroy (gameObject);
	}
}
