using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

	public GameObject crosshair;

	public Text moneyText;
	public Text focusText;

	public static  PlayerUI instance;

	// Use this for initialization
	void Awake () {
		instance = this;

	}
	
	// Update is called once per frame
	void Update () {
		if (moneyText != null) {
			moneyText.text = "Money: " + GameManager.instance.money.ToString ("F0") + "€";
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
