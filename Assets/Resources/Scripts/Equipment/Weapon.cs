using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Weapon : Equipment {

	[Header ("Damage")]
	public float slashDamage = 1;
	public float bluntDamage = 1;
	public float piercingDamage = 1;

	public float woodcuttingBonus = 0;
	public float miningBonus = 0;
	public float harvestingBonus = 0;

	public override void Start() {
		base.Start ();
		timeBetweenShots = 60 / rpm;
	}

	public virtual void ShootPrimary() {
		
	}
}
