using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(LivingEntity))]
public class ResourceExperienceDrop : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();

		if (GUILayout.Button ("Homo")) {
			//myScript.Die ();
		}
	}
}

