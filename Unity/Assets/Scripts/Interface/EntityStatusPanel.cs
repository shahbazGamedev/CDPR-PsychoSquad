using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Entity status panel prefab assigned to team panel.
/// </summary>
public class EntityStatusPanel : MonoBehaviour {
	public Button charButton;
	public Image portraitImage;
	public Image statusBar;
	public Image healthBar;
	private int index = -1;
	private Entity entityRef;

	/// <summary>
	/// Init the UI setup for specified entity and index.
	/// </summary>
	/// <param name="e">Entity.</param>
	/// <param name="ndx">Index.</param>
	public void Set(Entity e, int ndx) {
		index = ndx;
		portraitImage.sprite = e.portrait;
		entityRef = e;

		GameManager.OnEntityEvent += OnEntityEvent;
		GameManager.OnGameEvent += OnGameEvent;
	}

	void OnDisable() {
		GameManager.OnEntityEvent -= OnEntityEvent;
		GameManager.OnGameEvent -= OnGameEvent;
	}

	#region Event Listeners
	void OnEntityEvent(EntityEvent e) {
		if (e.eventObject != entityRef.gameObject) return;

		if (e.eventType == EntityEventType.Damaged) {
			SetHealth(entityRef.Health/100f);
		} else if (e.eventType == EntityEventType.MoveComplete) {
			SetStatus(entityRef.MoveRemaining/entityRef.MoveRange);
		} else if (e.eventType == EntityEventType.Death) {
			SetAlive(false);
		} else if (e.eventType == EntityEventType.Selected) {
			SetStatus(entityRef.MoveRemaining/entityRef.MoveRange);
		}
	}

	void OnGameEvent(GameEvent e) {
		if (e.eventTeam != entityRef.Team) return;

		if (e.eventType == GameEventType.TurnStart) {
			SetStatus(entityRef.MoveRemaining/entityRef.MoveRange);
		}
	}

	public void CharacterSelected() {
		GameManager.EntitySelected(index);
	}
	#endregion

	#region UI Update Methods
	void SetHealth(float health) {
		if (healthBar == null) return;
		healthBar.rectTransform.localScale = new Vector3(1f, Mathf.Clamp01(health), 1f);
	}
	void SetStatus(float status) {
		if (statusBar == null) return;
		statusBar.rectTransform.localScale = new Vector3(1f, Mathf.Clamp01(status), 1f);
	}
	void SetAlive(bool alive) {
		if (charButton == null) return;
		charButton.interactable = alive;
	}
	#endregion
}
