using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

	public GameObject crosshair;

	// Bars
	public Transform fatiqueBar;
	public Transform staminaBar;
	public Transform hungerBar;
	// Bar scales
	private Vector3 fatiqueBarScale;
	private Vector3 staminaBarScale;
	private Vector3 hungerBarScale;
	public Text moneyText;
	public Text focusText;

	public static  PlayerUI instance;
	private PlayerStats playerStats;

	// Use this for initialization
	void Awake () {
		instance = this;
	}

	void Start() {
		playerStats = GetComponent<PlayerStats> ();

		if (fatiqueBar != null) {
			fatiqueBarScale = fatiqueBar.localScale;
			staminaBarScale = staminaBar.localScale;
			hungerBarScale = hungerBar.localScale;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (moneyText != null) {
			moneyText.text = "Money: " + GameManager.instance.money.ToString ("F0") + "€";
		}
		if (staminaBar != null) {
			UpdateStaminaBar ();
			UpdateHungerBar ();
		}
	}

	public void UpdateFatiqueBar () {
		if (fatiqueBar != null) {
			fatiqueBar.localScale = new Vector3 (fatiqueBarScale.x * (playerStats.fatique / 100), fatiqueBarScale.y, fatiqueBarScale.z);
		}
	}

	public void UpdateStaminaBar() {
		if (staminaBar != null) {
			staminaBar.localScale = new Vector3 (staminaBarScale.x * (playerStats.stamina / 100), staminaBarScale.y, staminaBarScale.z);
		}
	}

	public void UpdateHungerBar() {
		if (hungerBar != null) {
			hungerBar.localScale = new Vector3 (hungerBarScale.x * (playerStats.hunger / 100), hungerBarScale.y, hungerBarScale.z);
		}
	}

	public void ShowCrosshair (bool show) {
		if (show) {
			crosshair.SetActive (true);
		} else {
			crosshair.SetActive (false);
		}
	}
}
