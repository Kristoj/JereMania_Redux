﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildInteractable : ChildEntity {

	public virtual void OnClientStartInteraction(string masterId) {
		if (parentEntity != null) {
			parentEntity.SetAuthority ();
		}
	}
}
