using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Enemy : LivingEntity {

	// Use this for initialization
	public override void Start () {

		health = startingHealth;

		//string myID = GetComponent<NetworkIdentity> ().netId.ToString ();
		//LivingEntity entity = GetComponent<LivingEntity> ();
		//GameManager.RegisterEntity (myID, entity, prefix);

		destroyOnDeath = true;
	}
}
