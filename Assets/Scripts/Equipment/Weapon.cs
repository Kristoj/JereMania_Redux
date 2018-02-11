using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Weapon : Equipment {

	public enum FireMode {Auto, Semi}
	[Header("Weapon Properties")]
	public FireMode fireMode;
	public float rpm = 100;
	protected float lastShotTime;
	protected float timeBetweenShots;

	[Header ("Damage")]
	public float slashDamage = 6;
	public float bluntDamage = 6;
	public float piercingDamage = 6;

	public float woodcuttingBonus = 0;
	public float miningBonus = 0;
	public float harvestingBonus = 0;

	[Header("FX")]
	public ImpactFX impactFX;

	[Header("Audio")]
	public AudioClip attackSound;

	public override void Start() {
		base.Start ();
		timeBetweenShots = 60 / rpm;
	}

	public override void Attack() {
		if (primaryAction == ActionType.Attack) {
			ShootPrimary ();
		} else {
			base.OnPrimaryAction ();
		}
	}

	public virtual void ShootPrimary() {
		
	}
}
