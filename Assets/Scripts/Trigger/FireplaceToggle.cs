using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceToggle : ChildInteractable {

	private Fireplace fireplace;

	void Start() {
		if (parentEntity != null) {
			fireplace = parentEntity.GetComponent<Fireplace> ();
		}
	}

	public override void OnClientStartInteraction(string masterId) {
		base.OnClientStartInteraction (masterId);

		if (fireplace != null) {
			fireplace.ToggleFire ();
		}
	}
}
