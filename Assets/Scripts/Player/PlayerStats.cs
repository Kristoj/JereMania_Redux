using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

	public float maxFatique = 100f;
	public float fatiqueDrainRate = .1f;
	public float maxStamina = 100f;
	public float staminaRechargeRate = 5f;
	public float staminaDrainRate = 10f;
	public float maxHunger = 100f;
	public float hungerDrainRate = .2f;
	public float fatique;
	public float stamina;
	public float hunger;

	// Bools
	private bool canRechargeStamina = true;

	// Classes
	private PlayerUI playerUI;
	private PlayerController playerController;
	private Player player;

	// Misc
	private Coroutine staminaDrainCoroutine;
	private Coroutine staminaRechargeDelayCoroutine;

	void Start() {
		playerUI = GetComponent<PlayerUI> ();
		playerController = GetComponent<PlayerController> ();
		player = GetComponent<Player> ();
		fatique = maxFatique;
		stamina = maxStamina;
		hunger = maxHunger;
	}

	void Update() {
		DrainBaseStats ();
		if (playerController.curSpeed == playerController.moveSpeed && canRechargeStamina) {
			stamina += staminaRechargeRate * Time.deltaTime;
			stamina = Mathf.Clamp (stamina, 0, maxStamina);
		}
	}

	// Add fatique
	public void FatiqueAdd(float f) {
		fatique += f;
		fatique = Mathf.Clamp (fatique, 0, maxFatique);

		playerUI.UpdateFatiqueBar ();
	}

	// Remove fatique
	public void FatiqueRemove(float f) {
		fatique -= f;
		fatique = Mathf.Clamp (fatique, 0, maxFatique);

		playerUI.UpdateFatiqueBar ();
	}

	// Add hunger
	public void HungerAdd (float f) {
		hunger += f;
		hunger = Mathf.Clamp (hunger, 0, maxHunger);

		playerUI.UpdateFatiqueBar ();
	}

	// Remove hunger
	public void HungerRemove(float f) {
		hunger -= f;
		hunger = Mathf.Clamp (hunger, 0, maxHunger);

		playerUI.UpdateFatiqueBar ();
	}

	public void StaminaRemove (float f, bool useRechargeDelay) {
		stamina -= f;
		stamina = Mathf.Clamp (stamina, 0, maxStamina);

		if (useRechargeDelay) {
			if (staminaRechargeDelayCoroutine != null) {
				StopCoroutine (staminaRechargeDelayCoroutine);
			}
			staminaRechargeDelayCoroutine = StartCoroutine (StaminaRechargeDelay());
		}
		playerUI.UpdateStaminaBar ();
	}

	// Add fatique
	public void StaminaAdd(float f) {
		stamina += f;
		stamina = Mathf.Clamp (stamina, 0, maxStamina);

		playerUI.UpdateStaminaBar ();
	}

	// Drain stamina
	public void StaminaDrain (bool ab) {
		if (ab) {
			if (staminaDrainCoroutine != null) {
				StopCoroutine (staminaDrainCoroutine);
			}
			staminaDrainCoroutine = StartCoroutine (StaminaBurn ());
		}
	}

	IEnumerator StaminaBurn() {
		while (playerController.curSpeed == playerController.runSpeed) {
			stamina -= staminaDrainRate * Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator StaminaRechargeDelay() {
		canRechargeStamina = false;
		yield return new WaitForSeconds (1f);
		canRechargeStamina = true;
	}

	void DrainBaseStats() {
		hunger -= hungerDrainRate * Time.deltaTime;
		fatique -= fatiqueDrainRate * Time.deltaTime;

		hunger = Mathf.Clamp (hunger, 0, maxHunger);
		fatique = Mathf.Clamp (fatique, 0, maxFatique);

		if (fatique <= 0 || hunger <= 0) {
			player.KillPlayer ();
		}
	}
}
