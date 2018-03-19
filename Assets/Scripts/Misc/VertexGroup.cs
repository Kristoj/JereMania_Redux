using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VertexGroup {

	[HideInInspector]
	public List<GroupVertex> groupVerts = new List<GroupVertex> ();
	[HideInInspector]
	public Vector3 groupOffset;
	[HideInInspector]
	public Vector3 groupOriginalPos;
	[HideInInspector]
	public float groupWeight = 1f;

	[System.Serializable]
	public class GroupVertex {
		public Vector3 jitterPos;
		public int groupIndex;
	}
}
