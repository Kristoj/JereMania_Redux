using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveChunk : MonoBehaviour {

	public CaveShape caveShape;
	public enum CaveShape {Straight, Left, Right, T, X, End, Start}

	// Use this for initialization
	void Start () {
		
	}
}