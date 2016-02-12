using UnityEngine;
using System.Collections;

/// <summary>
/// Human team manager. These are our players.
/// </summary>
public class HumanTeamManager : TeamManager {
	bool waitingForTarget = false;
	Entity[] enemies;

	#region Initialization
	public override void Initialize () {
		CreateTeam();
	}

	public override void CreateTeam ()
	{
		// squad setup
		GameObject _unit;

		for (int i = 0; i < GameManager.Instance.SquadRoster.Length; i++) {
			_unit = Instantiate(GameManager.Instance.SquadRoster[i]) as GameObject;
			_unit.transform.position = GameManager.spawns[TeamID].GetSpawnPos(i);
			_unit.name = GameManager.Instance.SquadRoster[i].name;

			Entity u = _unit.GetComponent<Entity>();
			if (u != null) {
				u.Init(this);

				entities.Add(u);
				GameManager.Register(u, i);
			}
		}
	}
	#endregion

	#region Update Methods
	public override void UpdateFunction () {
		// should receive an update from the active team when their turn is over
		if (CurrentPhase == TurnPhase.Wait) {
			if (GameManager.ActionMenu.activeSelf) GameManager.ActionMenu.SetActive (false);

		} else if (CurrentPhase == TurnPhase.TakeTurn) {
			if (!GameManager.ActionMenu.activeSelf) GameManager.ActionMenu.SetActive (true);
			if (!validEntityIndex) NextEntity();

			TurnUpdate();
			
		} else if (CurrentPhase == TurnPhase.EndTurn) {
			if (GameManager.ActionMenu.activeSelf) GameManager.ActionMenu.SetActive (false);
			
			EndUpdate();
		}
	}

	public override void WaitUpdate() {}

	public override void TurnUpdate() {
		// do we have an active selected unit?
		if (validEntityIndex) {
			if (FollowMouse.Active) {
				// update mouse cursor for movement
				if (!waitingForTarget) {
					FollowMouse.SetIcon(FollowMouse.IconType.Move);

					if (!currentEntity.Brain.HasMoved && (currentEntity.Position-FollowMouse.PointerPosition).sqrMagnitude <= (currentEntity.MoveRemaining*currentEntity.MoveRemaining)) {
						FollowMouse.SetColor(Color.green);
					} else {
						FollowMouse.SetColor(Color.red);
					}
				} else {
					FollowMouse.SetIcon(FollowMouse.IconType.Attack);
					// update cursor for targeting
					if (enemies != null && enemies.Length > 0) {
						// target pos in range?
						if (Toolbox.DistanceCheck(FollowMouse.PointerPosition, currentEntity.Position, currentEntity.Weapon.GetWeaponRange()) &&
							currentEntity.Weapon.HasAmmoLoaded()) {
							FollowMouse.SetColor(Color.white);

							// look for a nearby target
							Entity e = GetClosestTargetToClick();
							if (e != null) {
								if (Toolbox.DistanceCheck(FollowMouse.PointerPosition, e.Position, currentEntity.Weapon.GetWeaponRange()) &&
									currentEntity.Senses.CanSee(e.gameObject)) {
									FollowMouse.SetColor(Color.green);
								} else {
									FollowMouse.SetColor(Color.red);
								}
							}
						} else {
							FollowMouse.SetColor(Color.red);
						}
					}
				}
			}

			if (currentEntity.isMoving || currentEntity.isAttacking) return;

			if (currentEntity.Brain.HasMoved) {
				// if we've moved the currentEntity, but not as far as it can go, show the move ring again
				if (currentEntity.MoveRemaining >= 1f && !waitingForTarget) {
					FollowMouse.Active = true;
					FollowMouse.SetIcon(FollowMouse.IconType.Move);
				}
			}

			// update button interactability
			GameManager.GameCanvas.moveButton.interactable = !currentEntity.Brain.HasMoved;
			GameManager.GameCanvas.attackButton.interactable = !currentEntity.Brain.HasAttacked && currentEntity.Weapon.HasAmmoLoaded();
			GameManager.GameCanvas.reloadText.color = ((!currentEntity.Brain.HasAttacked || currentEntity.Weapon.HasAmmoLoaded()) ? Color.white : Color.green);
			GameManager.GameCanvas.specialButton.interactable = currentEntity.Ability.isAvailable;

			// if we've done both this turn with currentEntity, end the turn
			if (currentEntity.Brain.HasMoved && currentEntity.Brain.HasAttacked && !currentEntity.Ability.isAvailable) {
				currentEntity.CompleteTurn();
			}
		}
	}

	public override void EndUpdate() {
		// in case our turn timed out
		if (currentEntity != null) currentEntity.CompleteTurn();

		// reset all of our units
		foreach (Entity bu in Entities) {
			if (bu != null) {
				bu.TurnReset ();
			}
		}
		
		// notify other team it is their turn
		SelectEntity(-1);
		GameManager.ActivateNextTeam();
		CurrentPhase = TurnPhase.Wait;
	}
	#endregion

	#region Helper Methods
	void DoMove() {
		//Debug.Log("Received left click for move.");
		if (validEntityIndex && !currentEntity.isMoving && !currentEntity.Brain.HasMoved) {
			Vector3 pos = FollowMouse.PointerPosition;

			float d = (pos-currentEntity.Position).magnitude;
			if (currentEntity.MoveRemaining < d) return;

			currentEntity.MoveRemaining -= d;
			currentEntity.isMoving = true;
			currentEntity.Brain.AddGoal_MoveToPosition(pos);
		}
	}

	void DoAttack() {
		//Debug.Log("Received left click for attack.");
		Entity target = GetClosestTargetToClick();

		if (target == null) return;

		waitingForTarget = false;
		currentEntity.Brain.AddGoal_SelectTarget(target);
		currentEntity.Brain.QueueGoal_Attack();
	}

	Entity GetClosestTargetToClick() {
		if (enemies == null || enemies.Length < 1) return null;

		Vector3 terrainPos = FollowMouse.PointerPosition;
		float dist = 1.5f, d = 0f;
		Entity near = null;

		for (int i = 0; i < enemies.Length; i++) {
			if ((d = (terrainPos-enemies[i].Position).sqrMagnitude) <= dist) {
				near = enemies[i];
				dist = d;
			}
		}

		return near;
	}
	#endregion

	#region Action Selection Methods
	// when movement is selected for current unit in UI
	public override void MoveSelect () {
		// update to move cursor
		FollowMouse.SetIcon(FollowMouse.IconType.Move);
		FollowMouse.SetColor(Color.green);
		waitingForTarget = false;
	}
	
	// when attack is selected for current unit in UI
	public override void AttackSelect () {
		// update to attack cursor
		if (validEntityIndex && !currentEntity.Brain.HasAttacked) {
			FollowMouse.SetIcon(FollowMouse.IconType.Attack);
			FollowMouse.SetColor(Color.red);

			// update our enemies
			enemies = GameManager.GetEnemiesInRange(currentEntity, currentEntity.Weapon.GetWeaponRange());

			waitingForTarget = true;
		}
	}

	// when skip is selected for current unit in UI
	public override void SkipSelect () {
		base.SkipSelect();
	}

	// selecting the next unit
	public override void NextEntity () {
		base.NextEntity();
	}

	public override void SelectEntity (int i) {
		base.SelectEntity(i);

		if (currentEntity != null) {
			waitingForTarget = false;
			currentEntity.isMoving = false;

			FollowMouse.target = currentEntity;
			FollowMouse.Active = true;
			FollowMouse.SetIcon(FollowMouse.IconType.Move);
		}
	}

	public override void LeftClick() {
		if (waitingForTarget) {
			DoAttack();
		} else {
			DoMove();
		}
	}

	public override void RightClick() {
		MoveSelect();
	}
	#endregion
}
