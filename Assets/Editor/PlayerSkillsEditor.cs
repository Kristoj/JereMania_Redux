using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSkills))]
public class PlayerSkillsEditor : Editor {

	public static List<PlayerSkills.Skill> skillsEditor = new List<PlayerSkills.Skill> ();

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		// Draw areas for each skill
		for (int i = 0; i < skillsEditor.Count; i++) {
			if (GUILayout.Button (skillsEditor [i].skillName)) {

			}
		}

		if (GUILayout.Button ("Add Skill")) {
			skillsEditor.Add(new PlayerSkills.Skill());
		}

		if (GUILayout.Button ("Remove Skill")) {
			if (skillsEditor.Count > 0) {
				skillsEditor.RemoveAt (skillsEditor.Count - 1);
				Debug.Log (skillsEditor.Count);
			}
		}

	}
}
