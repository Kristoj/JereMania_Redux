using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExperienceDropTable : MonoBehaviour {

	[HideInInspector]
	public List<ExperienceDrop> xpDrops = new List<ExperienceDrop>();

	void Start() {
		//Entity e = GetComponent<Entity> ();
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
		public float dropAmount = 0;
	}
}
