using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Npc : LivingEntity {

	public float pathUpdateRate = .15f;

	[Header ("Combat")]
	public int damage = 25;
	public float attackSpeed = 1.3f;
	public float meleeRange = 2f;
	public float hitDelay = .25f;
	public LayerMask meleeMask;
	private bool canAttack = true;
	private bool isStaggerred = false;

	[Header ("Ragdoll")]
	public Rigidbody[] ragdollRb;

	private Animator animator;
	private NavMeshAgent agent;
	private Transform target;

	public override void Start() {
		base.Start ();
		animator = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();

		if (ragdollRb.Length > 0) {
			foreach (Rigidbody r in ragdollRb) {
				r.isKinematic = true;
			}
		}

		StartCoroutine (UpdatePath ());
		//Die ();
	}

	void Update() {
		animator.SetFloat ("moveSpeed", agent.velocity.magnitude);
	}

	public override void TakeDamage (float damage, string masterId) {
		base.TakeDamage (damage, masterId);
		animator.SetTrigger ("Stagger");
		StartCoroutine (Stagger ());
	}

	public override void Die() {
		base.Die ();
		agent.enabled = false;
		animator.enabled = false;

		if (ragdollRb.Length > 0) {
			foreach (Rigidbody r in ragdollRb) {
				r.isKinematic = false;
			}
		}
	}

	IEnumerator UpdatePath() {
		while (!dead) {
			yield return new WaitForSeconds (pathUpdateRate);

			if (dead) {
				yield break;
			}
			target = GameManager.GetLocalPlayer().transform;
			if (target != null) {
				agent.SetDestination (target.position);

				if (Vector3.Distance (transform.position, target.position) <= meleeRange && canAttack && !isStaggerred) {
					StartCoroutine (Attack ());
				}
			}
		}
	}

	IEnumerator Attack() {

		// Animations
		animator.SetTrigger ("Attack");
		canAttack = false;
		yield return new WaitForSeconds (hitDelay);

		if (isStaggerred || dead) {
			yield return null;
		}

		// Raycast to target
		Ray ray = new Ray (transform.position, target.position - transform.position);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, meleeRange, meleeMask)) {
			if (hit.collider.tag == "Player" || hit.collider.tag == "LocalPlayer") {
				hit.collider.GetComponent<LivingEntity> ().TakeDamage (damage, transform.name);
			}
		}

		yield return new WaitForSeconds (attackSpeed);
		canAttack = true;
	}

	IEnumerator Stagger() {
		agent.speed = 0f;
		agent.velocity = Vector3.zero;
		isStaggerred = true;
		animator.SetLayerWeight (1, 1);
		yield return new WaitForSeconds (1f);
		animator.SetLayerWeight (1, 0);
		isStaggerred = false;
		agent.speed = 4.5f;
	}
}
