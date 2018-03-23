using UnityEngine;
using UnityEditor;

public class PM_EditorWindow : EditorWindow {

	GameObject gm;
	Editor gmEditor;

	[MenuItem("ViliGay/Elä kurki")]
	static void ShowWindow() {
		GetWindowWithRect<PM_EditorWindow> (new Rect (0, 0, 512, 512));
	}

	void OnGUI() {
		gm = (GameObject)EditorGUILayout.ObjectField (gm, typeof(GameObject), true);

		GUIStyle bgColor = new GUIStyle ();
		bgColor.normal.background = EditorGUIUtility.whiteTexture;

		if (gm != null) {
			if (gmEditor == null) {
				gmEditor = Editor.CreateEditor (gm);
			}
			gmEditor.OnInteractivePreviewGUI (GUILayoutUtility.GetRect (512, 512), bgColor);
		}
	}
}
