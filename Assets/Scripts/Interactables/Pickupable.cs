using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Pickupable : Interactable {

	[Header("Main Properties")]
	public bool fixedRotation = false;
	public float carrySpeed = 5f;

	// Classes
	private PlayerInteraction playerInteraction;
	private Player player;
}