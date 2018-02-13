using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(AutomaticObjectAssignerScript))]
public class AutomaticObjectAssignerEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		AutomaticObjectAssignerScript myScript = target as AutomaticObjectAssignerScript;
		
		if (GUILayout.Button ("Automaticly Assign All Objects")) {
			myScript.AssignObjects ();
		}

	}
}
