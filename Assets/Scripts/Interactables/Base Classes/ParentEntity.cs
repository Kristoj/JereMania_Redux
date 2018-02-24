using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEntity : Entity {

	public List<ChildEntity> childEntities = new List<ChildEntity> ();

	public void RegisterChildInteractable(ChildEntity ci) {
		if (!childEntities.Contains(ci)) {
			childEntities.Add (ci);
		}
	}
}
