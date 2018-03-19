using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexJitter : MonoBehaviour {

	[Header ("Jitter")]
	public float jitterSpeed = 5f;
	public float jitterFrequency = .05f;
	public float jitterRange = .1f;

	[Header ("Scale")]
	public float meshScale = 1f;
	private float targetScale = 1;
	public float scaleSpeed = .5f;
	public float scaleFrequency = 1f;

	[Header("Weight")]
	public float[] materialsWeights;

	[Header ("Options")]
	public bool isEnabled = true;
	public bool canJitter = true;
	public bool canColorize = false;
	public bool canScale = false;

	[Header ("Misc")]
	[HideInInspector]
	public Vector3[] v;
	[HideInInspector]
	public List<VertexGroup> vg = new List<VertexGroup> ();
	[HideInInspector]
	public List<Vector3> uniqueCoord = new List<Vector3> ();

	// Color
	[HideInInspector]
	public Color[] vc;

	// Classes
	public MeshFilter filter;
	public MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
		//filter = GetComponent<MeshFilter> ();
		//meshRenderer = GetComponent<MeshRenderer>();
		//GroupVertices ();
		SetMaterialWeights ();
		// Coroutines
		if (canScale) {
			StartCoroutine (ScaleMesh ());
		}
		StartCoroutine (Jitter ());
		if (canColorize) {
			StartCoroutine (UpdateColor ());
		}
	}

	// Group our vertices
	public void GroupVertices() {
		// Init
		filter = GetComponent<MeshFilter> ();
		meshRenderer = GetComponent<MeshRenderer>();
		v = filter.sharedMesh.vertices;
		vc = new Color[filter.sharedMesh.vertices.Length];

		// Add all unique coords to our vertex group list
		for (int i = 0; i < v.Length; i++) {
			foreach (Vector3 targetV in v) {
				if (!uniqueCoord.Contains (v [i])) {
					uniqueCoord.Add (v [i]);
				}
			}
		}

		// Resize our vertex group list
		for (int i = 0; i < uniqueCoord.Count; i++) {
			vg.Add (new VertexGroup ());
		}

		// Assign vertices to groups
		for (int i = 0; i < v.Length; i++) {
			for (int j = 0; j < uniqueCoord.Count; j++) {
				// If target vertex has the same position as the target unique position, add it to our vertex group
				if (v [i] == uniqueCoord [j]) {
					vg [j].groupVerts.Add (new VertexGroup.GroupVertex());
					vg [j].groupOriginalPos = uniqueCoord[j];
					vg [j].groupVerts[vg[j].groupVerts.Count-1].groupIndex = i;
				}
			}
		}
	}

	public void SetMaterialWeights() {
		for (int i = 0; i < filter.sharedMesh.subMeshCount; i++) {
			Debug.Log (filter.sharedMesh.GetTriangles(i));
		}

	}

	public int GetVertexIndexByCoordinate(Vector3 coord) {
		for (int i = 0; i < vg.Count; i++) {
			if (vg [i].groupOriginalPos == coord) {
				return vg [i].groupVerts [0].groupIndex;
			}
		}
		return 0;
	}

	// Jitter
	IEnumerator Jitter() {
		float t = jitterFrequency;
		while (isEnabled) {
			if (canJitter) {
				// Randomize new group offset after x amount of seconds
				if (t >= jitterFrequency) {
					for (int i = 0; i < vg.Count; i++) {
						for (int j = 0; j < vg [i].groupVerts.Count; j++) {
							vg [i].groupOffset.x = Random.Range (-jitterRange, jitterRange);
							vg [i].groupOffset.y = Random.Range (-jitterRange, jitterRange);
							vg [i].groupOffset.z = Random.Range (-jitterRange, jitterRange);
						}
					}
					t = 0;
				}

				// Lerp our group vertices to the target position
				for (int i = 0; i < vg.Count; i++) {
					for (int j = 0; j < vg [i].groupVerts.Count; j++) {
						// Lerp the target vertex jitter pos to (original position + groups randomized offset)
						vg [i].groupVerts [j].jitterPos = Vector3.Lerp (vg [i].groupVerts [j].jitterPos, vg [i].groupOriginalPos + vg [i].groupOffset, jitterSpeed * Time.deltaTime);
						// Store our target vertex jitter pos to our vertice array
						v [vg [i].groupVerts [j].groupIndex] = vg [i].groupVerts [j].jitterPos;
					}
				}

				// Apply new vertices and recalculate bounds
				filter.mesh.vertices = v;
				//filter.mesh.RecalculateBounds ();

				t += Time.deltaTime;
			}
			yield return null;
		}
	}

	// Update vertex colors
	IEnumerator UpdateColor() {

		// Setup colors
		for (int i = 0; i < vc.Length; i++) {
			vc [i] = Color.red;
		}

		float maxDst = (vg [0].groupOriginalPos - (vg [0].groupOriginalPos + new Vector3 (jitterRange, jitterRange, jitterRange))).magnitude;
		while (isEnabled) {
			if (canColorize) {
				for (int i = 0; i < vg.Count; i++) {
					for (int j = 0; j < vg [i].groupVerts.Count; j++) {
						// Get our vertex distance from the original position
						Vector3 dir = v [vg[i].groupVerts [j].groupIndex] - vg [i].groupOffset;
						float dst = dir.magnitude;
						float percentage = dst / maxDst;
						float newColor = 255 * percentage;
						vc[vg[i].groupVerts[j].groupIndex].r = newColor;
					}
				}
				filter.mesh.colors = vc;
			}
			yield return null;
		}
	}

	// Scale the mesh
	IEnumerator ScaleMesh() {
		while (isEnabled) {
			if (canScale) {
				float t = scaleFrequency;
				Vector3 targetSize = Vector3.zero;
				if (t >= scaleFrequency) {
					targetScale = Random.Range (meshScale * .5f, meshScale * 1.5f);
					targetSize = new Vector3 (targetScale, targetScale, targetScale);
					t = 0;
				}
				transform.localScale = Vector3.Lerp (transform.localScale, targetSize, scaleSpeed * Time.deltaTime);
				t += Time.deltaTime;
			}
			yield return null;
		}
	}
}
