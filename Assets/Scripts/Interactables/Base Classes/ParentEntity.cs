using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEntity : Entity {

	public List<ChildInteractable> childInteractables = new List<ChildInteractable> ();

	public void RegisterChildInteractable(ChildInteractable ci) {
		if (!childInteractables.Contains(ci)) {
			childInteractables.Add (ci);
		}
	}
}
