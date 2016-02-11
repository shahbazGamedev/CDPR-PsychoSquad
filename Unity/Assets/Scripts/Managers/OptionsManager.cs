using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Options saving/loading and references.
/// </summary>
public class OptionsManager : Singleton<OptionsManager> {
	public GameObject optionsCanvas;
	public Slider gameSlider;
	public Slider musicSlider;
	public Slider uiSlider;
	public InputField nameField;
	public GameObject controlsPanel;
	public GameObject controlPanelPrefab;
	public RectTransform contentParent;
	public GameObject controlsButton;
	public float spacing = 50f;

	static private float gameVolume = 0.5f;
	static private float musicVolume = 0.5f;
	static private float uiVolume = 0.5f;
	static private string playerName = "Player";

	static private bool enableControls = true;
	static private bool settingControls = false;

	#region MonoBehaviour Methods
	void Start() {
		StartCoroutine(DelayedStart());
		StartSetup();
		EnableControlSetup(enableControls);
	}

	IEnumerator DelayedStart() {
		yield return new WaitForSeconds(1f);
		optionsCanvas.SetActive(false);
		yield return null;
	}
	#endregion

	#region Static Settings Methods
	static public void SetGameVolume(float v) {
		gameVolume = Mathf.Clamp01(v);
		AudioManager.UpdateGameVolume(gameVolume);
	}

	static public void SetMusicVolume(float v) {
		musicVolume = Mathf.Clamp01(v);
		AudioManager.UpdateMusicVolume(musicVolume);
	}

	static public void SetUIVolume(float v) {
		uiVolume = Mathf.Clamp01(v);
		AudioManager.UpdateUIVolume(uiVolume);
	}
	#endregion

	#region UI Listeners
	public void UpdateMusicVolume(float v) {
		SetMusicVolume(v);
	}

	public void UpdateGameVolume(float v) {
		SetGameVolume(v);
	}

	public void UpdateUIVolume(float v) {
		SetUIVolume(v);
	}

	public void SetPlayerName(string s) {
		playerName = s;
	}

	public void CloseButton() {
		// restore unsaved changes from player prefs
		float v = (PlayerPrefs.HasKey("GameVolume") ? PlayerPrefs.GetFloat("GameVolume") : 0.5f);
		SetGameVolume(v);
		v = (PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 0.5f);
		SetMusicVolume(v);
		v = (PlayerPrefs.HasKey("UIVolume") ? PlayerPrefs.GetFloat("UIVolume") : 0.5f);
		SetUIVolume(v);

		AudioManager.PlayBack();

		Close();
	}

	public void AcceptButton() {
		//audio
		PlayerPrefs.SetFloat("GameVolume", gameVolume);
		PlayerPrefs.SetFloat("MusicVolume", musicVolume);
		PlayerPrefs.SetFloat("UIVolume", uiVolume);

		//game
		PlayerPrefs.SetString("PlayerName", playerName);

		PlayerPrefs.Save();

		//Debug.Log("Saved preferences.");
		AudioManager.PlayConfirm();
		Close();
	}
	#endregion

	#region Setup
	public static void Show(bool show = true) {
		if (show) Instance.Open();
		else Instance.Close();
	}

	private void Open() {
		optionsCanvas.SetActive(true);

		// set sliders to proper positions
		Load();
	}

	private void Load() {
		//audio
		gameSlider.value = (PlayerPrefs.HasKey("GameVolume") ? Mathf.Clamp01(PlayerPrefs.GetFloat("GameVolume")) : 0.5f);
		musicSlider.value = (PlayerPrefs.HasKey("MusicVolume") ? Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume")) : 0.5f);
		uiSlider.value = (PlayerPrefs.HasKey("UIVolume") ? Mathf.Clamp01(PlayerPrefs.GetFloat("UIVolume")) : 0.5f);
		//game
		nameField.text = (PlayerPrefs.HasKey("PlayerName") ? PlayerPrefs.GetString("PlayerName") : "Player");
	}

	private void Close() {
		optionsCanvas.SetActive(false);

		PauseManager.ShowIfHidden();
	}

	private void StartSetup() {
		Load();
		UpdateMusicVolume(musicSlider.value);
		UpdateGameVolume(gameSlider.value);
		UpdateUIVolume(uiSlider.value);
	}
	#endregion

	#region Controls
	public static void EnableControlSetup(bool show = true) {
		//GameManager.Log("Control options enabled: " + show.ToString());
		enableControls = show;

		Instance.controlsButton.SetActive(show);
	}

	public void ShowControls() {
		Controls();
		settingControls = true;

		foreach (Transform tr in contentParent.transform) {
			Destroy(tr.gameObject);
		}

		for (int i = 0; i < cInput.length; i++) {
			GameObject go = Instantiate(controlPanelPrefab) as GameObject;
			RectTransform tr = go.GetComponent<RectTransform>();
			tr.SetParent(contentParent, false);
			tr.anchoredPosition = contentParent.anchoredPosition+(Vector2.down*spacing*(float)i);

			InputPanel ip = go.GetComponent<InputPanel>();
			if (ip != null) {
				ip.Set(cInput.GetText(i, 0), cInput.GetText(i, 1), cInput.GetText(i, 2), i);
			}

			contentParent.sizeDelta = new Vector2(contentParent.sizeDelta.x, spacing*(float)i);
		}
	}

	public static void ChangeKey(int i) {
		Transform child = Instance.contentParent.GetChild(i);
		InputPanel ip = child.GetComponent<InputPanel>();

		if (ip != null) {
			ip.Set(cInput.GetText(i, 0), cInput.GetText(i, 1), cInput.GetText(i, 2), i);
		}
	}

	public void HideControls() {
		Controls(false);
		settingControls = false;
	}

	public void Controls(bool show = true) {
		if (Instance.controlsPanel != null) Instance.controlsPanel.SetActive(show);
	}

	public void DefaultControls() {
		cInput.ResetInputs();
		ShowControls();
	}
	#endregion

	static public float GameVolume {
		get { return gameVolume; }
	}

	static public float MusicVolume {
		get { return musicVolume; }
	}

	static public string PlayerName {
		get { return playerName; }
	}

	static public bool SettingControls {
		get { return settingControls; }
	}
}
