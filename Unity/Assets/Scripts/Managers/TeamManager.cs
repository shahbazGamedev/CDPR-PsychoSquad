using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TurnPhase {
	Wait,
	TakeTurn,
	EndTurn
};

public enum TeamType {
	NONE = 0,
	Human,
	AI
}

public enum TeamFaction {
	NONE = 0,
	Squad,
	Psycho
}

/// <summary>
/// Team Manager abstract class that Human/AI teams inherit from.
/// </summary>
public abstract class TeamManager : MonoBehaviour {
	// tracking vars
	protected int teamID = -1;
	protected TeamFaction teamFaction = TeamFaction.Squad;
	protected TurnPhase turnPhase = TurnPhase.Wait;
	protected List<Entity> entities = new List<Entity>();
	protected int selectedEntity = -1;
	protected Entity currentEntity = null;
	protected int turnCount = 0;
	public bool takingTurn = false;

	public TeamType teamType = TeamType.NONE;

	#region MonoBehaviours
	public virtual void OnEnable() {
		GameManager.OnEntityEvent += HandleEntityEvent;
		GameManager.OnGameEvent += HandleGameEvent;
	}

	public virtual void OnDisable() {
		GameManager.OnEntityEvent -= HandleEntityEvent;
		GameManager.OnGameEvent -= HandleGameEvent;
	}

	public virtual void Awake () {
	}

	// Update is called once per frame
	public virtual void Update () {
		if (!GameManager.Running) return;

		UpdateFunction();
	}
	#endregion

	#region Event Listeners
	protected virtual void HandleEntityEvent(EntityEvent e) {
		// this would be a good location to listen for
		//	when a unit is finished moving, attacking, etc

		// when our current unit is finished with its turn
		if (e.eventTeam == this && e.eventObject != null) {
			Entity u = e.eventObject.GetComponent<Entity>();
			if (u != null && currentEntity == u && e.eventType == EntityEventType.TurnComplete) {
				// iterate to next available unit or end turn if last unit
				currentEntity.isPossessed = false;
				currentEntity.isSelected = false;

				NextEntity();
			}
		}
	}

	protected virtual void HandleGameEvent(GameEvent e) {
		if (e.eventTeam == this && e.eventType == GameEventType.TurnStart) {
			turnCount++;
		}
	}
	#endregion

	#region Abstract/Virtual Methods
	/// <summary>
	/// Called from game manager to setup the team after creation.
	/// </summary>
	public abstract void Initialize ();
	/// <summary>
	/// Called from GameManager to begin this team's turn.
	/// </summary>
	public virtual void StartTurn() {
		takingTurn = true;
		turnPhase = TurnPhase.TakeTurn;
		turnCount++;
	}
	/// <summary>
	/// Called every wait cycle.
	/// </summary>
	public abstract void WaitUpdate();
	/// <summary>
	/// Called every active turn cycle.
	/// </summary>
	public abstract void TurnUpdate();
	/// <summary>
	/// Called at the end of every turn.
	/// </summary>
	public abstract void EndUpdate();
	/// <summary>
	/// Cycles through turn phases.
	/// </summary>
	public virtual void UpdateFunction() {
		if (CurrentPhase == TurnPhase.Wait) WaitUpdate ();
		else if (CurrentPhase == TurnPhase.TakeTurn) TurnUpdate();
		else if (CurrentPhase == TurnPhase.EndTurn) EndUpdate();
	}
	/// <summary>
	/// Called by game manager when its ready for us to create our team. Team ID, etc should have already been assigned.
	/// </summary>
	public abstract void CreateTeam();

	// select a specific unit
	public virtual void SelectEntity (int i) {
		if (currentEntity != null && currentEntity.isSelected) {
			Debug.LogWarning ("Selected unit while the current was still working... Make sure units have finished their tasks first.");
			currentEntity.CompleteTurn();
		}

		selectedEntity = i;

		if (validEntityIndex) {
			currentEntity = entities[selectedEntity];

			// possess our unit so it can be controlled by the player
			if (teamType == TeamType.Human) {
				FollowMouse.target = currentEntity;
				FollowMouse.SetMode(true);
			}

			// highlight the selected unit
			//Debug.Log ("Selecting Entity " + i + ": " + currentEntity.name, currentEntity);

			currentEntity.SetSelected();

			SetUIForCharacter(currentEntity);

			currentEntity.BeginTurn();
		} else {
			currentEntity = null;
		}
	}

	/// <summary>
	/// Sets the UI display per character.
	/// </summary>
	/// <param name="u">Unit.</param>
	protected virtual void SetUIForCharacter(Entity u) {
		GameManager.GameCanvas.SetSpecialText(u.Ability.name);
	}

	// when movement is selected for current unit in UI
	public virtual void MoveSelect () {
		// update to move cursor
	}

	// when attack is selected for current unit in UI
	public virtual void AttackSelect () {
		// update to attack cursor
	}

	public virtual void SpecialSelect() {
		// update to special ability interface
		if (validEntityIndex && teamType == TeamType.Human) {
			currentEntity.UseSpecial();
		}
	}

	// when skip is selected for current unit in UI
	public virtual void SkipSelect () {
		// end current unit's turn
		if (validEntityIndex && teamType == TeamType.Human) {
			currentEntity.CompleteTurn();
		}
	}

	// when end is selected in UI
	public virtual void EndSelect() {
		if (teamType == TeamType.Human)
			CurrentPhase = TurnPhase.EndTurn;
	}

	// when reload is selected in UI
	public virtual void ReloadSelect() {
		if (validEntityIndex && teamType == TeamType.Human) {
			currentEntity.Brain.AddGoal_Reload();
		}
	}

	public virtual void LeftClick() {}
	public virtual void RightClick() {}

	// selecting the next unit
	public virtual void NextEntity () {
		selectedEntity++;
		Entity nbu = null;

		if (selectedEntity >= entities.Count) {
			// check if we have any Entitys that were skipped to go back over
			// if so, set selected unit to their index
			bool wrap = false;
			for (int i = 0; i < entities.Count; i++) {
				nbu = entities[i];
				if (nbu != null && !nbu.turnComplete && nbu.isAlive && !nbu.isStunned) {
					SelectEntity (i);
					wrap = true;
				}
			}
			if (!wrap) {
				// if not, end our turn
				SelectEntity (-1);
				CurrentPhase = TurnPhase.EndTurn;
			}
		} else {
			nbu = entities[selectedEntity];
			if (nbu != null && !nbu.turnComplete && nbu.isAlive) {
				SelectEntity (selectedEntity);
			} else {
				NextEntity ();
			}
		}
	}

	/// <summary>
	/// Entities alive on this team.
	/// </summary>
	/// <returns>The alive.</returns>
	public int EntitiesAlive() {
		int leftAlive = 0;

		for (int i = 0; i < entities.Count; i++) {
			if (entities[i] != null && entities[i].isAlive) leftAlive++;
		}

		return leftAlive;
	}
	#endregion

	#region Getters
	public TurnPhase CurrentPhase {
		get { return turnPhase; }
		set { turnPhase = value; }
	}
	public List<Entity> Entities {
		get { return entities; }
	}
	public int TeamID {
		get { return teamID; }
		set { teamID = value; }
	}
	public bool validEntityIndex {
		get { return (selectedEntity >= 0 && selectedEntity < entities.Count); }
	}
	public int TurnCount {
		get { return turnCount; }
		set { turnCount = value; }
	}
	public TeamFaction TeamFaction {
		get { return teamFaction; }
		set { teamFaction = value; }
	}
	#endregion
}
