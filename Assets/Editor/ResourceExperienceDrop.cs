using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(LivingEntity))]
public class ResourceExperienceDrop : Editor {

	public List<ExperienceDropTable> dropTables = new List<ExperienceDropTable>();

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		//LivingEntity myScript = target as LivingEntity;

		if (GUILayout.Button ("Homo")) {
			//myScript.Die ();
		}
	}
}

