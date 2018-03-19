using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ExperienceDropTable))]
public class ExperienceDropTableEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI ();

		ExperienceDropTable myScript = target as ExperienceDropTable;

		// List of experience drop menus
		GUILayout.BeginVertical ("textArea");
		for (int i = 0; i < myScript.xpDrops.Count; i++) {
			// Clone vars
			ExperienceDropTable.ExperienceDrop.SkillName skillEnum = myScript.xpDrops[i].skillName;
			ExperienceDropTable.ExperienceDrop.YieldType yieldEnum = myScript.xpDrops[i].yieldType;
			float dropAmount = myScript.xpDrops [i].dropAmount;
			GUILayout.Space (20);
			GUILayout.BeginVertical ("Window");
			GUILayout.BeginHorizontal ("Box", GUILayout.Width (100), GUILayout.MinHeight (40));

			EditorGUILayout.LabelField ("", GUILayout.MaxWidth(20));
			myScript.xpDrops[i].skillName = (ExperienceDropTable.ExperienceDrop.SkillName)EditorGUILayout.EnumPopup ("", skillEnum, GUILayout.MaxWidth (100));
			EditorGUILayout.LabelField ("", GUILayout.MaxWidth(20));


			EditorGUILayout.LabelField ("Drop Amount", GUILayout.MaxWidth(100));
			myScript.xpDrops[i].dropAmount = EditorGUILayout.FloatField (dropAmount, GUILayout.MaxWidth (40));


			EditorGUILayout.LabelField ("", GUILayout.MaxWidth(40));
			myScript.xpDrops[i].yieldType = (ExperienceDropTable.ExperienceDrop.YieldType)EditorGUILayout.EnumPopup ("", yieldEnum, GUILayout.MaxWidth (100));

			GUILayout.EndHorizontal ();

			// Remove button
			GUILayout.Space (5);
			GUILayout.BeginHorizontal ("textArea", GUILayout.Width (100));
			GUILayout.Space (200);
			if (GUILayout.Button ("Remove", GUILayout.MaxWidth (120))) {
				myScript.xpDrops.RemoveAt (i);
			}
			GUILayout.Space (200);
			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			GUILayout.Space (10);
		}
		GUILayout.EndVertical ();
		GUILayout.BeginHorizontal ("Box", GUILayout.Height (30));
		GUILayout.Space (200);
		if (GUILayout.Button ("Add", GUILayout.MaxWidth (120))) {
			myScript.xpDrops.Add (new ExperienceDropTable.ExperienceDrop ());
		}
		GUILayout.Space (200);
		GUILayout.EndHorizontal ();
	}
}