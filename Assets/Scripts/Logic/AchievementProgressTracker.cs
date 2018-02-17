using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementProgressTracker : MonoBehaviour {

	// Woodcutting
	public int treesChopped;
	public int firewoodChopped;
	// Mining
	public int rockMined;
	// Foraging
	public int mushroomsGathered;

	public static AchievementProgressTracker instance;

	void Awake() {
		instance = this;
		LoadPlayerProgress ();
	}

	public void LoadPlayerProgress() {
		treesChopped = UnityEngine.PlayerPrefs.GetInt ("treesChopped", 0);
		firewoodChopped = UnityEngine.PlayerPrefs.GetInt ("firewoodChopped", 0);
		rockMined = UnityEngine.PlayerPrefs.GetInt ("rockMined", 0);
		mushroomsGathered = UnityEngine.PlayerPrefs.GetInt ("mushroomsGathered", 0);
	}

	public void SavePlayerProgress() {
		UnityEngine.PlayerPrefs.SetInt ("treesChopped", treesChopped);
		UnityEngine.PlayerPrefs.SetInt ("firewoodChopped", firewoodChopped);
		UnityEngine.PlayerPrefs.SetInt ("rockMined", rockMined);
		UnityEngine.PlayerPrefs.SetInt ("mushroomsGathered", mushroomsGathered);
	}

	void OnApplicationQuit() {
		SavePlayerProgress ();
	}

	public void AddAchievementProgress(string entityName) {
		if (entityName == "Acacia" || entityName == "Maple") {
			treesChopped++;
		}

		else if (entityName == "Wood") {
			firewoodChopped++;
		}

		else if (entityName == "Ore") {
			rockMined++;
		}

		else if (entityName == "Mushroom") {
			mushroomsGathered++;
		}
	}
}
