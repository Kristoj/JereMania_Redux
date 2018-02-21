﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour {

	public List<Skill> skillList = new List<Skill> ();
	public static List<Skill> skills = new List<Skill> ();

	void Awake() {
		skills = skillList;
	}

	public static void AddExperience (float amount, string _skillName) {
		foreach (Skill s in skills) {
			if (s.skillName == _skillName) {
				s.AddExperienceToSkill ();
			}
		}
	}

	[System.Serializable]
	public class Skill {
		public string skillName = "";

		public void AddExperienceToSkill() {

		}
	}
}