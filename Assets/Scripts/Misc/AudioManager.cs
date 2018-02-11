using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AudioManager : NetworkBehaviour {

	public float masterVolume = 1;
	public float musicVolume = .8f;
	public float sfxVolume = 1f;
	public AudioClip defaultMusic;

	private AudioSource[] musicSources;
	private AudioSource sfx2DSource;
	private int activeMusicSourceIndex;

	public static AudioManager instance;
	private SoundLibrary soundLibrary;


	void Start() {

		instance = this;
		soundLibrary = GetComponent<SoundLibrary> ();

		musicSources = new AudioSource[2];
		for (int i = 0; i < 2; i++) {
			GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
			musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
			musicSources [i].GetComponent<AudioSource> ().loop = true;
			newMusicSource.transform.parent = transform;
		}
		GameObject newSFXSource = new GameObject ("SFX2D source");
		sfx2DSource = newSFXSource.AddComponent<AudioSource> ();
		sfx2DSource.transform.parent = transform;


		if (defaultMusic != null) {
			StartCoroutine (PlayMusicOnDelay (.2f));
		}
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
		activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicSources [activeMusicSourceIndex].clip = clip;
		musicSources[activeMusicSourceIndex].Play();

		StartCoroutine (AnimateMusicCrossfade (fadeDuration));
	}


	public void PlaySound (string soundName, Vector3 soundPos, bool abParent) {

		if (abParent) {
			GameObject newSFXSource = new GameObject ("SFX2D source");
			newSFXSource.transform.parent = transform;
			StartCoroutine (DestroyCustomSFXSource (newSFXSource));
		}
	}

	[Command]
	public void CmdPlayCustomSound (string clip, Vector3 soundPos, string masterId, float volume) {
		RpcPlayCustomSound (clip, soundPos, masterId, volume);
	}

	[Command]
	public void CmdPlayCustomSound2D (string clip, Vector3 soundPos, string masterId, float volume) {
		RpcPlayCustomSound2D (clip, soundPos, masterId, volume);
	}

	[ClientRpc]
	public void RpcPlayCustomSound (string clip,Vector3 soundPos, string masterId, float volume) {

		Transform sourcePlayer = null;
		if (GameManager.GetCharacter (masterId) != null) {
			sourcePlayer = GameManager.GetCharacter (masterId).transform;
		}
		if (sourcePlayer != null) {
			// Setup sound game object
			GameObject newSFXSource = new GameObject ("SFX2D source");
			AudioSource newAudioSource = newSFXSource.AddComponent<AudioSource> ();
			newSFXSource.transform.parent = transform;
			// Sound volume settings
			newAudioSource.maxDistance = 30;
			newAudioSource.spatialBlend = .96f;
			// Play audio and destroy it after X amount of time
			newAudioSource.PlayOneShot (soundLibrary.GetClip(clip), masterVolume * sfxVolume * volume);
			StartCoroutine (DestroyCustomSFXSource (newSFXSource));
		} else {
			AudioSource.PlayClipAtPoint (soundLibrary.GetClip(clip), soundPos, sfxVolume * masterVolume * volume);
		}
	}

	[ClientRpc]
	public void RpcPlayCustomSound2D (string clip,Vector3 soundPos, string masterId, float volume) {

		Transform sourcePlayer = null;
		if (GameManager.GetCharacter (masterId) != null) {
			sourcePlayer = GameManager.GetCharacter (masterId).transform;
		}

		if (GameManager.instance.localPlayer.name == masterId) {
			sfx2DSource.PlayOneShot (soundLibrary.GetClip (clip), masterVolume * sfxVolume * volume);
		} else {
			// Setup sound game object
			GameObject newSFXSource = new GameObject ("SFX2D source");
			AudioSource newAudioSource = newSFXSource.AddComponent<AudioSource> ();
			newAudioSource.transform.position = soundPos;
			newSFXSource.transform.parent = transform;
			// Sound volume settings
			newAudioSource.maxDistance = 30;
			newAudioSource.spatialBlend = .96f;
			// Play audio and destroy it after X amount of time
			newAudioSource.PlayOneShot (soundLibrary.GetClip (clip), masterVolume * sfxVolume * volume);
			StartCoroutine (DestroyCustomSFXSource (newSFXSource));
		}
	}

	public void PlayCustomSoundForDuration (AudioClip clip, Vector3 soundPosition, bool abParent, float duration) {

		if (abParent) {
			GameObject newSFXSource = new GameObject ("SFX2D source");
			AudioSource newAudioSource = newSFXSource.AddComponent<AudioSource> ();
			newAudioSource.maxDistance = 30;
			newAudioSource.spatialBlend = .96f;
			newAudioSource.PlayOneShot (clip, masterVolume * sfxVolume);
			newSFXSource.transform.parent = transform;
			StartCoroutine (DestroyCustomSFXSource (newSFXSource));
			StartCoroutine (StopAudio (newAudioSource, duration));
		} else {
			AudioSource.PlayClipAtPoint (clip, soundPosition, sfxVolume * masterVolume);
		}
	}

	public void PlaySound2D (AudioClip clip, float vol) {
		sfx2DSource.PlayOneShot (clip, masterVolume * sfxVolume * vol);
	}

	public void PlaySoundWithVolume (AudioClip clip, Vector3 soundPosition, float vol) {
		GameObject newSFXObject = new GameObject ("Custom SFX Source");
		newSFXObject.transform.position = soundPosition;
		AudioSource newSFXSource = newSFXObject.AddComponent<AudioSource> ();
		newSFXSource.clip = clip;
		newSFXSource.volume = sfxVolume * masterVolume * vol;
		newSFXSource.maxDistance = 30;
		newSFXSource.spatialBlend = .96f;
		newSFXSource.Play ();
		StartCoroutine (DestroyCustomSFXSource (newSFXObject));
	}

	IEnumerator DestroyCustomSFXSource(GameObject obj) {
		yield return new WaitForSeconds (10);
		Destroy (obj);
	}

	IEnumerator AnimateMusicCrossfade (float duration) {
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolume * masterVolume, percent);
			musicSources [1-activeMusicSourceIndex].volume = Mathf.Lerp (musicVolume * masterVolume, 0, percent);
			yield return null;
		}
	}

	IEnumerator StopAudio(AudioSource source, float duration) {
		yield return new WaitForSeconds (duration);
		source.Stop ();
	}


	IEnumerator PlayMusicOnDelay(float delay) {
		yield return new WaitForSeconds (delay);
		PlayMusic (defaultMusic);
	}
}
