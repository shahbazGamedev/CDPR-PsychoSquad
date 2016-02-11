using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Game canvas class that handles input/UI in game scene.
/// </summary>
public class GameCanvas : MonoBehaviour {
	public GameObject actionPanel;
	public Button moveButton;
	public Button attackButton;
	public Button specialButton;
	public Button skipButton;
	public Button endTurnButton;
	[Header("Character Status")]
	public RectTransform contentPanel;
	public GameObject charStatusPrefab;
	public GameObject teamPanel;

	#region UI Listener
	public void Pause() {
		PauseManager.Pause();
	}

	public void MovePressed() {
		GameManager.MoveSelected();
	}

	public void AttackPressed() {
		GameManager.AttackSelected();
	}

	public void SpecialPressed() {
		GameManager.SpecialSelected();
	}

	public void SkipPressed() {
		GameManager.SkipSelected();
	}

	public void ReloadPressed() {
		GameManager.ReloadSelected();
	}

	public void EndTurnPressed() {
		GameManager.EndTurnSelected();
	}
	#endregion

	#region Public Methods
	public void CreateEntityPanel(Entity e, int ndx) {
		if (contentPanel == null || charStatusPrefab == null) return;

		GameObject go = Instantiate(charStatusPrefab) as GameObject;
		go.transform.SetParent(contentPanel, false);

		EntityStatusPanel panel = go.GetComponentInChildren<EntityStatusPanel>();
		if (panel != null) {
			panel.Set(e, ndx);
		}
	}

	public void ClearStatusPanels() {
		if (contentPanel != null) {
			foreach (Transform tr in contentPanel) {
				Destroy(tr.gameObject);
			}
		}
	}

	public void ShowPanels(bool enable = true) {
		ShowActionPanel(enable);
		ShowCharacterPanel(enable);
	}
	public void ShowActionPanel(bool enable = true) {
		if (actionPanel != null) actionPanel.SetActive(enable);
	}
	public void ShowCharacterPanel(bool enable = true) {
		if (teamPanel != null) teamPanel.SetActive(enable);
	}

	public void SetSpecialText(string txt) {
		if (specialButton != null) {
			Text textComponent = specialButton.GetComponentInChildren<Text>();
			if (textComponent != null) {
				textComponent.text = txt;
			}
		}
	}
	#endregion
}
