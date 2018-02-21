using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSkills))]
public class PlayerSkillsEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		PlayerSkills ps = target as PlayerSkills;

		
		// Draw areas for each skill


		if (GUILayout.Button ("Add Skill")) {
			PlayerSkills.skills.Add(new PlayerSkills.Skill());
		}

		if (GUILayout.Button ("Remove Skill")) {
			if (PlayerSkills.skills.Count > 0) {
				PlayerSkills.skills.RemoveAt (PlayerSkills.skills.Count - 1);
				Debug.Log (PlayerSkills.skills.Count);
			}
		}
	}

	[System.Serializable]
	public class SkillEditor {
		public string skillName = "";
		public int skillLevel = 0;
		public int skillMaxLevel = 50;
	}
}
