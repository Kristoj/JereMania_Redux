using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon {

	[Header("Gun Properties")]
	public float spread = 5f;
	public float hipSpreadMultiplier = 3f;
	public int projectileCount = 1;

	[Header ("Kickback")]
	public float kickX = .005f;
	public float kickCeilX = .01f;
	public float kickSpeedX = 2;
	public float kickReturnSpeedX = 5;
	public float kickMinY = .01f;
	public float kickMaxY = .1f;
	public float kickCeilY = .1f;
	public float kickSpeedY = 10f;
	public float kickReturnSpeedY = 15f;
	public float kickMinZ = .1f;
	public float kickMaxZ = .2f;
	public float kickCeilZ = .3f;
	public float kickSpeedZ = 10f;
	public float kickReturnSpeedZ = 15f;

	[Header ("Recoil")]
	public float recoilX = 5f;
	public float recoilSpeedX = 8f;
	public float recoilMinY = 4f;
	public float recoilMaxY = 8f;
	public float recoilCeilY = 13f;
	public float recoilSpeedY = 15f;
	public float recoilReturnSpeed = 20f;

	[Header("FX")]
	public Tracer tracer;
	public float tracerSpeed = 50f;
}
