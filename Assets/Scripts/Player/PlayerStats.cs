using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

	public float maxFatique = 100f;
	public float fatique;
	private Vector3 originalSize;

	public Transform fatiqueBar;


	void Start() {
		fatique = maxFatique;

		if (fatiqueBar != null) {
			originalSize = fatiqueBar.localScale;
		}
	}

	public void AddFatique(float f) {
		fatique += f;
		fatique = Mathf.Clamp (fatique, 0, maxFatique);

		if (fatiqueBar != null) {
			fatiqueBar.localScale = new Vector3 (originalSize.x * (fatique / 100), originalSize.y, originalSize.z);
		}
	}

	public void RemoveFatique(float f) {
		fatique -= f;
		fatique = Mathf.Clamp (fatique, 0, maxFatique);

		if (fatiqueBar != null) {
			fatiqueBar.localScale = new Vector3 (originalSize.x * (fatique / 100), originalSize.y, originalSize.z);
		}
	}
}
