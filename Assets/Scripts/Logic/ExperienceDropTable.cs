using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExperienceDropTable : MonoBehaviour {

	[HideInInspector]
	public List<ExperienceDrop> xpDrops = new List<ExperienceDrop>();

	void Start() {
		Entity e = GetComponent<Entity> ();
		LivingEntity le = GetComponent <LivingEntity> ();
		if (e != null || le != null) {
			foreach (ExperienceDrop exp in xpDrops) {
				if (le != null) {
					if (exp.yieldType == ExperienceDrop.YieldType.OnDeath) {
						le.deathEvent += exp.AddExperience;
					}
				}
				if (e != null) {
					if (exp.yieldType == ExperienceDrop.YieldType.OnPickup) {
						e.pickupEvent += exp.AddExperience;
					}
				}
			}
		}
	}

	[System.Serializable]
	public class ExperienceDrop {
		[HideInInspector]
		public SkillName skillName;
		[HideInInspector]
		public enum SkillName {Woodcutting, Mining, Foraging}
		[HideInInspector]
		public YieldType yieldType;
		[HideInInspector]
		public enum YieldType {OnDeath, OnPickup}
		[HideInInspector]
		public int dropAmount = 0;

		public void AddExperience (string targetPlayer) {
			PlayerSkills.instance.AddExperienceToSkill (skillName.ToString (), dropAmount);
		}
	}
}
