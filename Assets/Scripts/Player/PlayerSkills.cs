using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour {
	
	public List<Skill> skills = new List<Skill> ();
	public static PlayerSkills instance;

	void Awake() {
		instance = this;
	}

	public void AddExperienceToSkill(string skillName, int amount) {
		foreach (Skill s in skills) {
			if (s.skillName == skillName) {
				Debug.Log (skillName.ToString ());
				s.AddExperienceToSkill (amount);
			}
		}
	}

	[System.Serializable]
	public class Skill {
		public string skillName = "Skill Name Here";
		public int skillLevel = 0;
		public int skillMaxLevel = 50;
		public int skillExperience = 0;

		public void AddExperienceToSkill(int amount) {
			skillExperience += amount;
		}
	}
}