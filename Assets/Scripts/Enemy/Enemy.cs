using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Enemy : LivingEntity {

	// Use this for initialization
	public override void Start () {

		health = startingHealth;

		destroyOnDeath = true;
	}
}
