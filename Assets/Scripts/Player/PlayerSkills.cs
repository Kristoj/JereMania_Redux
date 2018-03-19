using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour {
	
	public List<Skill> skills = new List<Skill> ();
	public static PlayerSkills instance;

	void Awake() {
		instance = this;
	}

	[System.Serializable]
	public class Skill {
		public string skillName = "Skill Name Here";
		public int skillLevel = 0;
		public int skillMaxLevel = 50;

		public void AddExperienceToSkill(float amount) {

		}
	}
}