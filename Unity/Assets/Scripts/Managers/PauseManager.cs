using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Pause manager of UI/input for game scene.
/// </summary>
public class PauseManager : Singleton<PauseManager> {
	public GameObject pauseCanvas;
	public Button restartButton;
	bool hidden = false;
	private static bool paused = false;

	void Start() {
		StartCoroutine(DelayedStart());
	}

	IEnumerator DelayedStart() {
		yield return new WaitForSeconds(1f);
		pauseCanvas.SetActive(false);
		yield return null;
	}

	public static void Pause(bool pause = true) {
		if (Instance.hidden) return;
		Instance.pauseCanvas.gameObject.SetActive(pause);
		paused = pause;
		GameManager.Pause(pause);
		AudioManager.Pause(pause);
		Cursor.visible = pause;
	}

	// if the pause menu is hidden behind another UI temporarily, reveal it
	public static void ShowIfHidden() {
		if (Instance.hidden) {
			Instance.pauseCanvas.SetActive(true);
			Instance.hidden = false;
		}
	}

	public static void EnableRestart(bool enable = true) {
		Instance.restartButton.interactable = enable;
	}

	#region UI Listeners
	public void Resume() {
		Pause(false);
		AudioManager.PlayBack();
	}

	public void Restart() {
		Pause(false);
		AudioManager.PlayClick();
		GameManager.Restart();
	}

	public void ReturnToMain() {
		Pause(false);
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

	public void Options() {
		OptionsManager.Show();
		AudioManager.PlayClick();
		hidden = true;
		Instance.pauseCanvas.SetActive(false);
	}

	public void Quit() {
		Application.Quit();
	}
	#endregion

	static public bool Paused {
		get { return paused; }
	}
}
