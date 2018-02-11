using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour {

	public SoundGroup[] soundGroups;
	public EntitySoundManager[] entitySoundManagers;
	public static SoundLibrary instance;

	//private Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();
	public AudioClip[] sounds;

	void Awake() {
		instance = this;
		foreach (SoundGroup soundGroup in soundGroups) {
			//groupDictionary.Add (soundGroup.groupId, soundGroup.group);
		}
	}

	/**
	public AudioClip GetClip (string name) {
		if (groupDictionary.ContainsKey (name)) {
			AudioClip[] sounds = groupDictionary [name];
			return sounds [Random.Range (0, sounds.Length)];
		} else {
			return null;
		}
	}
	**/

	public AudioClip GetClip (string clipName) {
		foreach (AudioClip a in sounds) {
			if (a.name == clipName) {
				return a;
			}
		}
		return null;
	}

	public AudioClip GetEntityImpactSound(string entityType, string equipmentType) {
		
		foreach (EntitySoundManager esm in entitySoundManagers) {
			if (entityType == esm.entityType) {
				return esm.GetBranchClip (equipmentType);
			}
		}
		return null;
	}

	[System.Serializable]
	public class SoundGroup {
		public string groupId;
		public AudioClip[] group;
	}
}
