using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main menu UI class, handles inputs and UI updates.
/// </summary>
public class MainMenu : MonoBehaviour {
	public GameObject quitObject;
	private bool starting = false;

	void Start() {
		AudioManager.PlayMenuMusic();
		ShowQuitDialogue(false);
		LoadingUtil.EndLoad();
	}

	public void PlayPressed() {
		if (starting) return;

		StartCoroutine(StartGame());

		AudioManager.PlayClick();
	}

	IEnumerator StartGame() {
		starting = true;

		LoadingUtil.BeginLoad();
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
		yield return null;
	}

	public void OptionsPressed() {
		OptionsManager.Show();

		AudioManager.PlayClick();
	}

	public void QuitPressed() {
		ShowQuitDialogue();

		AudioManager.PlayBack();
	}

	public void ShowQuitDialogue(bool show = true) {
		quitObject.SetActive(show);
	}

	public void ConfirmQuit() {
		Application.Quit();
	}

	public void CancelQuit() {
		ShowQuitDialogue(false);

		AudioManager.PlayConfirm();
	}
}
