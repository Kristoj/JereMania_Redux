using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralMesh))]
public class ProceduralMeshEditor : Editor {

	ProceduralMesh myScript;

	public override void OnInspectorGUI() {
		base.OnInspectorGUI ();

		myScript = target as ProceduralMesh;

		if (GUILayout.Button ("Generate Mesh")) {
			myScript.GenerateMesh ();
		}
		if (myScript != null) {
			Vector2 vsp = HandleUtility.WorldToGUIPoint (myScript.transform.position + myScript.v [0]);
			GUI.Label (new Rect (vsp.x, vsp.y, 100, 20), "v1");
			GUI.Button (new Rect (0, 0, 100, 100), "sss");
		}

	}

	void OnGUI() {

	}
}
