using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VertexJitter))]
public class VertexJitterEditor : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI ();
		VertexJitter myScript = target as VertexJitter;
		MeshRenderer mr = myScript.meshRenderer;

		// Set target mesh renderer if it's null
		if (myScript.meshRenderer == null) {
			myScript.meshRenderer = myScript.GetComponent<MeshRenderer> ();
			mr = myScript.meshRenderer;
			myScript.materialsWeights = new float[myScript.meshRenderer.sharedMaterials.Length];
		}
		EditorGUILayout.BeginVertical ("Box");
		// Material weight UI
		for (int i = 0; i < myScript.materialsWeights.Length; i++) {
			EditorGUILayout.BeginHorizontal ("textarea");
			// Material name
			EditorGUILayout.LabelField (mr.sharedMaterials[i].name, GUILayout.MaxWidth (50));
			EditorGUILayout.LabelField ("Weight", GUILayout.MaxWidth (50));
			myScript.materialsWeights [i] = EditorGUILayout.FloatField (myScript.materialsWeights[i], GUILayout.MaxWidth (25));
			EditorGUILayout.EndHorizontal ();
		}

		if (GUILayout.Button("Group Vertices")) {
			myScript.GroupVertices ();
		}
		EditorGUILayout.EndVertical ();
	}
}
