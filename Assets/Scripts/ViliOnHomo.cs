using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViliOnHomo : MonoBehaviour {

	private string viliMessage;

	void Start () {
		PrintMessage ();
	}

	void PrintMessage() {
		viliMessage = "krifiomo";
		Debug.Log (viliMessage);
	}
}
