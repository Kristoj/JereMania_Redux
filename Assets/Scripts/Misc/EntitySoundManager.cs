using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySoundManager : MonoBehaviour {

	public string entityType;
	public EntitySoundBranch[] entityBranches;

	public AudioClip GetBranchClip(string equipmentType) {
		foreach (EntitySoundBranch esb in entityBranches) {
			if (equipmentType == esb.branchName) {
				return esb.branchClips [Random.Range (0, esb.branchClips.Length)];
			}
		}
		return null;
	}

	[System.Serializable]
	public class EntitySoundBranch {
		public string branchName;
		public AudioClip[] branchClips;
	}
}
