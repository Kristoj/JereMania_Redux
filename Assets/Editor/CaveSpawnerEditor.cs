using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Cavespawner))]
public class CaveSpawnerEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		Cavespawner myScript = target as Cavespawner;

		if (GUILayout.Button ("Generate Cave")) {
			//myScript.caveSpawn ();
		}

		if (GUILayout.Button ("Clear Cave")) {
			//myScript.ClearCave ();
		}
	}
}