using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Loss manager for lose condition UI/input.
/// </summary>
public class LossManager : Singleton<LossManager> {
	public GameObject loseCanvas;

	// Use this for initialization
	void Start() {
		StartCoroutine(DelayedStart());
	}

	IEnumerator DelayedStart() {
		yield return new WaitForSeconds(1f);
		loseCanvas.SetActive(false);
		yield return null;
	}

	public void Restart() {
		StartCoroutine(DelayedHide(1f));
		AudioManager.PlayConfirm();
		GameManager.Restart();
	}

	IEnumerator DelayedHide(float d) {
		yield return new WaitForSeconds(d);
		Hide();
		yield return null;
	}

	public void Quit() {
		Hide();
		AudioManager.PlayBack();
		GameManager.AbortingGame();
		StartCoroutine(ToMain());
	}

	IEnumerator ToMain() {
		LoadingUtil.BeginLoad();
		yield return new WaitForSeconds(1f);
		GameManager.StopRoundTime();
		SceneManager.LoadScene("Menu");
	}

	public static void Show() {
		Instance.loseCanvas.SetActive(true);
		AudioManager.Pause();
		GameManager.Pause();
	}

	public static void Hide() {
		Instance.loseCanvas.SetActive(false);
		AudioManager.Pause(false);
		GameManager.Pause(false);
	}
}
