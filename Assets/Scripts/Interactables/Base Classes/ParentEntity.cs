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

	public ChildEntity GetChildEntity(string entityType, string childName) {
		foreach (ChildEntity c in childEntities) {
			if (c.GetType().ToString() == entityType.ToString() && c.name == childName) {
				return c;
			}
		}
		return null;
	}
}
