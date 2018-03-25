using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType {
	Quad, 
	Cube
}

public class ProceduralMesh : MonoBehaviour {

	[Header("Mesh")]
	public MeshType meshType;
	public MeshFilter filter;
	public float width = 1;
	public float height = 1;
	public float depth = 1;

	[Header("Destruction")]
	public float holeSize = .1f;
	public int holeVertexCount = 8;

	// Data
	public Vector3[] v;
	Vector3[] n;
	Vector2[] uvs;
	int[] tris;

	public void GenerateMesh() {

		// Reference mesh filter
		if (filter == null) {
			filter = GetComponent<MeshFilter> ();
		}

		// Clear the mesh before generating new vertices
		if (filter.sharedMesh != null) {
			filter.sharedMesh.Clear ();
		}

		if (filter.sharedMesh == null) {
			filter.sharedMesh = new Mesh ();
		}

		switch (meshType.ToString()) {
		case ("Quad"):
			GenerateQuad ();
			break;
		case ("Cube"):
			GenerateCube ();
			break;
		default:
			break;
		}
	}

	void GenerateCube() {
		// Set new vertices
		v = new Vector3[24];
		// Front
		v [0] = new Vector3 (-width / 2, -height/2, -depth/2);
		v [1] = new Vector3 (-width / 2, height/2, -depth/2);
		v [2] = new Vector3 (width / 2, height/2, -depth/2);
		v [3] = new Vector3 (width / 2, -height/2, -depth/2);
		// Left
		v [4] = new Vector3 (-width / 2, -height/2, depth/2);
		v [5] = new Vector3 (-width / 2, height/2, depth/2);
		v [6] = new Vector3 (-width / 2, height/2, -depth/2);
		v [7] = new Vector3 (-width / 2, -height/2, -depth/2);
		// Back
		v [8] = new Vector3 (width / 2, -height/2, depth/2);
		v [9] = new Vector3 (width / 2, height/2, depth/2);
		v [10] = new Vector3 (-width / 2, height/2, depth/2);
		v [11] = new Vector3 (-width / 2, -height/2, depth/2);
		// Right
		v [12] = new Vector3 (width / 2, -height/2, -depth/2);
		v [13] = new Vector3 (width / 2, height/2, -depth/2);
		v [14] = new Vector3 (width / 2, height/2, depth/2);
		v [15] = new Vector3 (width / 2, -height/2, depth/2);
		// Top
		v [16] = new Vector3 (-width / 2, height/2, -depth/2);
		v [17] = new Vector3 (-width / 2, height/2, depth/2);
		v [18] = new Vector3 (width / 2, height/2, depth/2);
		v [19] = new Vector3 (width / 2, height/2, -depth/2);
		// Bottom
		v [20] = new Vector3 (-width / 2, -height/2, -depth/2);
		v [21] = new Vector3 (-width / 2, -height/2, depth/2);
		v [22] = new Vector3 (width / 2, -height/2, depth/2);
		v [23] = new Vector3 (width / 2, -height/2, -depth/2);
		filter.sharedMesh.vertices = v;

		// Normals
		n = new Vector3[24];
		// Front
		n[0] = new Vector3 (0,0,-1);
		n[1] = new Vector3 (0,0,-1);
		n[2] = new Vector3 (0,0,-1);
		n[3] = new Vector3 (0,0,-1);
		// Left
		n[4] = new Vector3 (-1,0,0);
		n[5] = new Vector3 (-1,0,0);
		n[6] = new Vector3 (-1,0,0);
		n[7] = new Vector3 (-1,0,0);
		// Back
		n[8] = new Vector3 (0,0,1);
		n[9] = new Vector3 (0,0,1);
		n[10] = new Vector3 (0,0,1);
		n[11] = new Vector3 (0,0,1);
		// Right
		n[12] = new Vector3 (1,0,0);
		n[13] = new Vector3 (1,0,0);
		n[14] = new Vector3 (1,0,0);
		n[15] = new Vector3 (1,0,0);
		// Top
		n[16] = new Vector3 (0,1,0);
		n[17] = new Vector3 (0,1,0);
		n[18] = new Vector3 (0,1,0);
		n[19] = new Vector3 (0,1,0);
		// Bottom
		n[20] = new Vector3 (0,-1,0);
		n[21] = new Vector3 (0,-1,0);
		n[22] = new Vector3 (0,-1,0);
		n[23] = new Vector3 (0,-1,0);
		filter.sharedMesh.normals = n;

		// UVS
		uvs = new Vector2[24];
		// Front
		uvs[0] = new Vector2 (0, 0); uvs[1] = new Vector2 (0, 1); 
		uvs[2] = new Vector2 (1, 1); uvs[3] = new Vector2 (1, 0);
		// Left
		uvs[4] = new Vector2 (0, 0); uvs[5] = new Vector2 (0, 1);
		uvs[6] = new Vector2 (1, 1); uvs[7] = new Vector2 (1, 0);
		// Back
		uvs[8] = new Vector2 (0, 0); uvs[9] = new Vector2 (0, 1);
		uvs[10] = new Vector2 (1, 1); uvs[11] = new Vector2 (1, 0);
		// Right
		uvs[12] = new Vector2 (0, 0); uvs[13] = new Vector2 (0, 1);
		uvs[14] = new Vector2 (1, 1); uvs[15] = new Vector2 (1, 0);
		// Top
		uvs[16] = new Vector2 (0, 0); uvs[17] = new Vector2 (0, 1);
		uvs[18] = new Vector2 (1, 1); uvs[19] = new Vector2 (1, 0);
		// Bottom
		uvs[20] = new Vector2 (0, 0); uvs[21] = new Vector2 (0, 1);
		uvs[22] = new Vector2 (1, 1); uvs[23] = new Vector2 (1, 0);
		filter.sharedMesh.uv = uvs;

		tris = new int[36];
		// Front
		tris [0] = 0; tris [1] = 1; tris [2] = 2; 
		tris [3] = 0; tris [4] = 2; tris [5] = 3;
		// Left
		tris [6] = 4; tris [7] = 5; tris [8] = 6;
		tris [9] = 4; tris [10] = 6; tris [11] = 7;
		// Back
		tris [12] = 8; tris [13] = 9; tris [14] = 10;
		tris [15] = 8; tris [16] = 10; tris [17] = 11;
		// Right
		tris [18] = 12; tris [19] = 13; tris [20] = 14; 
		tris [21] = 12; tris [22] = 14; tris [23] = 15;
		// Top
		tris [24] = 16; tris [25] = 17; tris [26] = 18;
		tris [27] = 16; tris [28] = 18; tris [29] = 19;
		// Bottom
		tris [30] = 20; tris [31] = 21; tris [32] = 22;
		tris [33] = 20; tris [34] = 22; tris [35] = 23;
		filter.sharedMesh.triangles = tris;
	}

	// Generate Quad
	void GenerateQuad() {
		// Set our new vertices
		v = new Vector3[4];
		v[0] = new Vector3 (-width/2, -height/2, 0);
		v[1] = new Vector3 (-width/2, height/2, 0);
		v[2] = new Vector3 (width/2, height/2, 0);
		v[3] = new Vector3 (width/2, -height/2, 0);
		filter.sharedMesh.vertices = v;

		// Normals
		n = new Vector3[4];
		n [0] = new Vector3( 0, 0, -1);
		n [1] = new Vector3( 0, 0, -1);
		n [2] = new Vector3( 0, 0, -1);
		n [3] = new Vector3( 0, 0, -1);
		filter.sharedMesh.normals = n;

		// Uvs
		uvs = new Vector2[4];
		uvs [0] = new Vector2 (0, 0);
		uvs [1] = new Vector2 (0, 1);
		uvs [2] = new Vector2 (1, 1);
		uvs [3] = new Vector2 (1, 0);
		filter.sharedMesh.uv = uvs;

		// Set Triangles
		tris = new int[6];
		tris [0] = 0;
		tris [1] = 1;
		tris [2] = 2;
		tris [3] = 0;
		tris [4] = 2;
		tris [5] = 3;
		filter.sharedMesh.triangles = tris;
	}
}
