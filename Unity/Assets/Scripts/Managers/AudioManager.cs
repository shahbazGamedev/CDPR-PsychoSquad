using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// Audio manager class that handles all game audio.
/// </summary>
public class AudioManager : Singleton<AudioManager> {
	[Header("Mixer References")]
	public AudioMixer mixer;
	public AudioMixerGroup musicGroup;
	public AudioMixerGroup gameGroup;
	public AudioMixerGroup uiGroup;
	public AudioMixerSnapshot gameplay;
	public AudioMixerSnapshot paused;

	[Header("UI SFX")]
	public AudioSource uiSource;
	public AudioClip menuClick;
	public AudioClip menuSelect;
	public AudioClip menuBack;

	[Header("Music")]
	public AudioSource musicSource;
	public AudioClip menuTrack;
	public AudioClip[] gameTracks;
	private bool playingGameTrack = false;
	public AudioClip deathTune;
	public AudioClip victoryTune;

	[Header("Game SFX")]
	public int gameChannels = 16;
	private AudioSource[] gameSources;
	public AudioClip[] assaultNear;
	public AudioClip[] assaultFar;
	public AudioClip[] smgNear;
	public AudioClip[] smgFar;
	public AudioClip[] sniperNear;
	public AudioClip[] sniperFar;
	public AudioClip[] pistolNear;
	public AudioClip[] pistolFar;
	public AudioClip[] lmgNear;
	public AudioClip[] lmgFar;


	// Use this for initialization
	void Start () {
		if (uiSource != null) {
			uiSource.loop = false;
			uiSource.playOnAwake = false;
			uiSource.spatialBlend = 0f;
			uiSource.Stop ();

			uiSource.outputAudioMixerGroup = uiGroup;
		}

		if (musicSource != null) {
			musicSource.loop = true;
			musicSource.playOnAwake = false;
			musicSource.spatialBlend = 0f;
			musicSource.Stop();

			musicSource.outputAudioMixerGroup = musicGroup;
		}

		// create game source channels
		GameObject go = new GameObject("GameAudio");
		go.transform.parent = transform;

		gameSources = new AudioSource[gameChannels];
		for (int i = 0; i < gameChannels; i++) {
			AudioSource s = go.AddComponent<AudioSource>();
			s.loop = false;
			s.playOnAwake = false;
			s.spatialBlend = 0f;
			s.Stop ();

			s.outputAudioMixerGroup = gameGroup;

			gameSources[i] = s;
		}
	}

	#region Public Methods
	public static void UpdateGameVolume(float v) {
		for (int i = 0 ; i < Instance.gameSources.Length; i++) {
			Instance.gameSources[i].volume = v;
		}
	}

	public static void UpdateMusicVolume(float v) {
		Instance.musicSource.volume = v;
	}

	public static void UpdateUIVolume(float v) {
		Instance.uiSource.volume = v;
	}

	public static void Pause(bool pause = true) {
		AudioMixerSnapshot[] snaps = new AudioMixerSnapshot[2];
		snaps[0] = (pause ? Instance.paused : Instance.gameplay);
		snaps[1] = (pause ? Instance.gameplay : Instance.paused);

		Instance.mixer.TransitionToSnapshots(snaps, new float[2]{1f, 0f}, 1f);
	}
	#endregion

	#region Music
	public static void PauseMusic(bool pause = true) {
		if (Instance.musicSource.isPlaying && pause) {
			Instance.musicSource.Pause();
		} else if (!Instance.musicSource.isPlaying && !pause) {
			Instance.musicSource.UnPause();
		}
	}

	public static void PlayVictory() {
		Instance.musicSource.Stop();
		Instance.musicSource.clip = Instance.victoryTune;
		Instance.musicSource.loop = false;
		Instance.musicSource.Play();
	}

	public static void PlayFail() {
		Instance.musicSource.Stop();
		Instance.musicSource.clip = Instance.deathTune;
		Instance.musicSource.loop = false;
		Instance.musicSource.Play();
	}

	// TODO: implement these methods with fading in/out
	public static void PlayMenuMusic() {
		Instance.playingGameTrack = false;

		Instance.musicSource.Stop();
		Instance.musicSource.clip = Instance.menuTrack;
		Instance.musicSource.loop = true;
		Instance.musicSource.Play();
	}

	public static void PlayGameMusic(int difficulty) {
		Instance.playingGameTrack = true;
		// TODO: change this back if we don't want random game music
		difficulty = Random.Range(0,Instance.gameTracks.Length);
		if (Instance.musicSource.clip != Instance.gameTracks[difficulty]) {
			Instance.musicSource.Stop();
			//Instance.musicSource.clip = Instance.gameTracks[Random.Range(0,Instance.gameTracks.Length)];
			Instance.musicSource.clip = Instance.gameTracks[difficulty];	// difficulty-based music selection
			Instance.musicSource.loop = true;								// repeating for difficulty-based

			Instance.StartCoroutine(FadeIn(Instance.musicSource));
			Instance.musicSource.Play();
		} else if (!Instance.musicSource.isPlaying) {
			Instance.musicSource.Play();
		}
		//Instance.StartCoroutine(PlayNextGameTrack(Instance.musicSource.clip.length));
	}

	static IEnumerator PlayNextGameTrack(float d, int difficulty) {
		yield return new WaitForSeconds(d-1f);
		Instance.StartCoroutine(FadeOut(Instance.musicSource));
		yield return new WaitForSeconds(1f);

		if (Instance.playingGameTrack)
			PlayGameMusic(difficulty);
		yield return null;
	}

	static IEnumerator FadeIn(AudioSource src) {
		src.volume = 0f;

		while (src.volume < OptionsManager.MusicVolume) {
			src.volume += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		src.volume = OptionsManager.MusicVolume;
		yield return null;
	}

	static IEnumerator FadeOut(AudioSource src) {
		while (src.volume > 0f) {
			src.volume = Mathf.Clamp01(src.volume-Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}

		yield return null;
	}
	#endregion

	#region Game Audio
	/// <summary>
	/// Plays the provided clip, and returns the audio source in case of looping so that it can be stopped or modified later.
	/// </summary>
	/// <returns>The AudioSource the clip is using.</returns>
	/// <param name="clip">Clip.</param>
	/// <param name="loop">If set to <c>true</c> loop.</param>
	public static AudioSource PlayClip(AudioClip clip, bool loop = false) {
		// find the first available channel to play this clip
		AudioSource retSource = null;
		for (int i = 0 ; i < Instance.gameSources.Length; i++) {
			if (Instance.gameSources[i].isPlaying) continue;
			//Debug.Log("Playing audio source " + i.ToString());

			ResetToDefault(Instance.gameSources[i]);
			Instance.gameSources[i].clip = clip;
			Instance.gameSources[i].loop = loop;
			Instance.gameSources[i].Play();

			retSource = Instance.gameSources[i];
			break;
		}

		return retSource;
	}

	public static void StopClip(AudioClip clip) {
		for (int i = 0 ; i < Instance.gameSources.Length; i++) {
			if (Instance.gameSources[i].clip == clip && Instance.gameSources[i].isPlaying) {
				Instance.gameSources[i].Stop();
				//break;
			}
		}
	}

	public static void StopAll() {
		for (int i = 0; i < Instance.gameSources.Length; i++) {
			Instance.gameSources[i].Stop();
		}
	}

	/// <summary>
	/// Resets the default source for game sfx.
	/// </summary>
	/// <param name="s">S.</param>
	private static void ResetToDefault(AudioSource s) {
		s.loop = false;
		s.playOnAwake = false;
		s.spatialBlend = 0f;
		s.volume = OptionsManager.GameVolume;
	}

	public static void PlayAR(bool near = true) {
		PlayClip((near ? Instance.assaultNear[Random.Range(0, Instance.assaultNear.Length)] : Instance.assaultFar[Random.Range(0, Instance.assaultFar.Length)]), false);
	}

	public static void PlaySMG(bool near = true) {
		PlayClip((near ? Instance.smgNear[Random.Range(0, Instance.smgNear.Length)] : Instance.smgFar[Random.Range(0, Instance.smgFar.Length)]), false);
	}

	public static void PlaySniper(bool near = true) {
		PlayClip((near ? Instance.sniperNear[Random.Range(0, Instance.sniperNear.Length)] : Instance.sniperFar[Random.Range(0, Instance.sniperFar.Length)]), false);
	}

	public static void PlayPistol(bool near = true) {
		PlayClip((near ? Instance.pistolNear[Random.Range(0, Instance.pistolNear.Length)] : Instance.pistolFar[Random.Range(0, Instance.pistolFar.Length)]), false);
	}

	public static void PlayLMG(bool near = true) {
		PlayClip((near ? Instance.lmgNear[Random.Range(0, Instance.lmgNear.Length)] : Instance.lmgFar[Random.Range(0, Instance.lmgFar.Length)]), false);
	}
	#endregion

	#region  UI Audio
	public static void PlayUIClip(AudioClip clip) { 
		Instance.uiSource.Stop();
		Instance.uiSource.clip = clip;
		Instance.uiSource.Play();
	}

	public static void PlayClick() {
		PlayUIClip(Instance.menuClick);
	}

	public static void PlayBack() {
		PlayUIClip(Instance.menuBack);
	}

	public static void PlayConfirm() {
		PlayUIClip(Instance.menuSelect);
	}
	#endregion

	#region Getters
	public static AudioMixerGroup GameMix {
		get { return Instance.gameGroup; }
	}

	public static AudioMixerGroup MusicMix {
		get { return Instance.musicGroup; }
	}
	#endregion
}
