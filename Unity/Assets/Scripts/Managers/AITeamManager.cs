using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI team for NPC cycles.
/// </summary>
public class AITeamManager : TeamManager {
	public float maxTurnTime = 30f;
	private float turnTimeElapsed = 0f;
	private bool cycling = false;

	#region Initialization
	public override void Initialize() {
		CreateTeam();
	}

	public override void CreateTeam() { 
		// psycho setup
		GameObject _unit;
		bool isSquad = (teamID%2 == 0);
		name = (isSquad ? "Squad" : "Pyscho") + " Team " + teamID.ToString();
		GameObject[] roster = (isSquad ? GameManager.Instance.SquadRoster : GameManager.Instance.PsychoRoster);

		for (int i = 0; i < roster.Length && i <= (int)GameManager.difficulty; i++) {
			_unit = Instantiate(roster[i]) as GameObject;
			_unit.transform.position = GameManager.spawns[TeamID].GetSpawnPos(i);
			_unit.name = roster[i].name;

			Entity u = _unit.GetComponent<Entity>();
			if (u != null) {
				u.Init(this);

				entities.Add(u);
				GameManager.Register(u);
			}
		}
	}
	#endregion

	#region Update Methods
	public override void WaitUpdate() {}

	public override void TurnUpdate() {
		if (GameManager.WaitCanvas != null && !GameManager.WaitCanvas.gameObject.activeSelf) GameManager.WaitCanvas.Show();

		if (!cycling) {
			cycling = true;

			// this will automatically start the process of the AI,
			//	by utilizing callbacks it will proceed through the team
			//	on its own
			NextEntity();
		}

		turnTimeElapsed += Time.deltaTime;
		if (turnTimeElapsed >= maxTurnTime) {
			CurrentPhase = TurnPhase.EndTurn;
		}
	}

	public override void EndUpdate() {
		cycling = false;

		// in case our turn timed out
		if (currentEntity != null) currentEntity.CompleteTurn();

		if (GameManager.WaitCanvas != null && GameManager.WaitCanvas.gameObject.activeSelf) GameManager.WaitCanvas.Show(false);

		// reset all units
		foreach (Entity u in Entities) {
			u.TurnReset();
		}

		SelectEntity (-1);
		turnTimeElapsed = 0f;
		GameManager.ActivateNextTeam();
		CurrentPhase = TurnPhase.Wait;
	}
	#endregion
}
