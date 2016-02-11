using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Logo/splash screen class that displays an image for a
/// period of time before transitioning to next build scene.
/// </summary>
public class Logo : MonoBehaviour {
	public float delay = 3f;

	public AudioClip[] clips;
	public float[] audioDelays;

	// Use this for initialization
	void Start () {
		LoadingUtil.EndLoad();

		if (clips != null && clips.Length > 0) {
			if (audioDelays.Length != clips.Length-1) {
				audioDelays = new float[clips.Length-1];
				for (int i = 0; i < audioDelays.Length; i++) {
					audioDelays[i] = 0f;
				}
			}
		}

		StartCoroutine(NextScene());
		StartCoroutine(PlayClips());
	}

	IEnumerator NextScene() {
		yield return new WaitForSeconds(Mathf.Max(delay-1f,1f));
		LoadingUtil.BeginLoad();
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
	}

	IEnumerator PlayClips() {
		if (clips == null || clips.Length == 0) yield break;

		for (int i = 0; i < clips.Length; i++) {
			AudioManager.PlayUIClip(clips[i]);
			if (clips.Length > i+1)
				yield return new WaitForSeconds(audioDelays[i]);
		}
		yield return null;
	}
}
