using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController : Entity {

	//Update is called once per frame
	void Update () {
		if(owner.name == GameManager.GetLocalPlayer().name){
			print ("Jere");

		}

	}
}
