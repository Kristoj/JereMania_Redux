using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildInteractable : MonoBehaviour {

	public Entity parentEntity;

	public virtual void Start() {
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
				Entity e = targetObject.parent.GetComponent<Entity> ();
				if (e != null) {
					parentEntity = e;
					pass = true;
				} else {
					if (targetObject.parent != null) {
						targetObject = targetObject.parent;
					}
				}
				Debug.Log ("loop");
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
