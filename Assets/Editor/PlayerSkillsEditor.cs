using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerSkills))]
public class PlayerSkillsEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		PlayerSkills myScript = target as PlayerSkills;

		// Draw areas for each skill
		for (int i = 0; i < myScript.skills.Count; i++) {

			// Begin body
			GUILayout.BeginVertical ("Box");
			GUILayout.Space (10);
			GUILayout.BeginHorizontal ("textArea");
			GUILayout.Space (200);
			myScript.skills[i].skillName = EditorGUILayout.TextField (myScript.skills[i].skillName, GUILayout.MaxWidth (100));
			GUILayout.Space (150);
			GUILayout.EndHorizontal ();

			// Start inner body
			GUILayout.BeginVertical("Box", GUILayout.MaxWidth (60), GUILayout.MinHeight (30));
			GUILayout.BeginHorizontal ("TextArea", GUILayout.MaxWidth (120), GUILayout.MinHeight (40));
			GUILayout.Space (200);
			myScript.skills[i].skillLevel = EditorGUILayout.IntField ("Level", myScript.skills[i].skillLevel, "Box", GUILayout.MinHeight (20));
			GUILayout.Space (200);
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			// Remove button
			if (GUILayout.Button ("Remove")) {
				myScript.skills.RemoveAt (i);
			}
			GUILayout.Space (10);
			GUILayout.EndVertical ();
			// End body
		}

		if (GUILayout.Button ("Add Skill")) {
			myScript.skills.Add(new PlayerSkills.Skill());
		}
	}
}
