using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour {

	public SoundGroup[] soundGroups;

	//private Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();
	public AudioClip[] sounds;

	void Awake() {
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

	[System.Serializable]
	public class SoundGroup {
		public string groupId;
		public AudioClip[] group;
	}
}
