using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildInteractable : MonoBehaviour {

	public ParentEntity parentEntity;

	public virtual void Awake() {
		// Get the parent entity
		FindParentEntity ();
		// Register this to the parent entity
		RegisterToParent ();
	}

	void FindParentEntity() {
		/**
		if (transform.parent != null) {
			Entity e = transform.parent.GetComponent<Entity> ();
			if (e != null) {
				parentEntity = e;
			} else {
				if (transform.parent.transform.parent != null) {
					FindParentEntity ();
				}
			}
		}
		**/
		Transform targetObject = transform;
		bool pass = false;
		while (targetObject != null && !pass) {
			if (targetObject.parent != null) {
				ParentEntity e = targetObject.parent.GetComponent<ParentEntity> ();
				if (e != null) {
					parentEntity = e;
					pass = true;
				} else {
					if (targetObject.parent != null) {
						targetObject = targetObject.parent;
					}
				}
			} else {
				pass = true;
			}
		}
	}



	void RegisterToParent() {
		if (parentEntity != null) {
			parentEntity.SendMessage ("RegisterChildInteractable",this as ChildInteractable, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void OnClientStartInteraction(string masterId) {
		if (parentEntity != null) {
			parentEntity.SetAuthority ();
		}
	}
}
