using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Exploration goal designed to move progressively towards enemies while sticking together with teammates.
/// </summary>
public class GoalExplore : CompositeGoal {
	private Vector3 m_CurrentDestination;
	private bool m_bDestinationIsSet;
	private bool m_bCalculatingDestination;

	public GoalExplore(Entity unit):base(unit, GoalType.Move) {
		m_bDestinationIsSet = m_bCalculatingDestination = false;
	}

	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		RemoveAllSubgoals();

		if (!m_Entity.Agent.enabled) m_Entity.EnableAgent();
	}

	public override int Process ()
	{
		ActivateIfInactive();

		if (!m_Entity.Agent.enabled) {
			m_iStatus = (int)Status.Running;
		} else if (!m_bDestinationIsSet && !m_bCalculatingDestination) {
			GetDestination();
			if (m_bDestinationIsSet) {
				AddSubgoal (new GoalSeekToPosition(m_Entity, m_CurrentDestination));
			} else {
				m_iStatus = (int)Status.Failed;
			}
		} else {
			m_iStatus = ProcessSubgoals();

			if (m_Subgoals.Count > 0 && m_iStatus != (int)Status.Failed) {
				m_iStatus = (int)Status.Running;
			}
		}

		return m_iStatus;
	}

	Vector3 GetExploreDestination() {
		Vector3 dest = m_Entity.Position;
		List<Entity> team = new List<Entity>();
		List<Entity> enemy = new List<Entity>();

		// sorting into teams
		for (int i = 0; i < GameManager.Entities.Length; i++) {
			if (m_Entity != GameManager.Entities[i] && GameManager.Entities[i].Team == m_Entity.Team) {
				team.Add(GameManager.Entities[i]);
			} else {
				enemy.Add (GameManager.Entities[i]);
			}
		}

		// tracking count of team members
		int c = 0;

		// compute teammate average position
		Vector3 tAvg = Vector3.zero;
		if (team.Count > 0) {		
			c = 0;
			for (int i = 0; i < team.Count; i++) {
				if (team[i].isAlive) {
					tAvg += team[i].Position;
					c++;
				}
			}
			if (c > 0)
				tAvg = tAvg * (1f/(float)c);
		}

		// compute enemy average position
		Vector3 eAvg = Vector3.zero;
		if (enemy.Count > 0) {
			c = 0;
			for (int i = 0; i < enemy.Count; i++) {
				if (enemy[i].isAlive) {
					eAvg += enemy[i].Position;
					c++;
				}
			}
			if (c > 0)
				eAvg = eAvg * (1f/(float)c);
		}

		// build a direction from those two averaged positions for an average direction
		Vector3 aDir = Vector3.zero;
		if (tAvg != Vector3.zero && eAvg != Vector3.zero)	aDir = (((tAvg+eAvg)*0.5f)-m_Entity.Position).normalized*1.5f;	// weighting direction strongly towards center of friendly/enemy positions
		else if (tAvg == Vector3.zero && eAvg != Vector3.zero)	aDir = (m_Entity.Position-eAvg).normalized*0.5f;			// slight weight towards moving away from enemy position
		else if (eAvg == Vector3.zero && tAvg != Vector3.zero)	aDir = (tAvg-m_Entity.Position).normalized;					// standard weight to move towards the center of our team

		// add some randomization
		Vector3 rDir = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y).normalized;
		// final direction
		Vector3 fDir = (aDir+rDir).normalized;

		dest += (fDir*m_Entity.MoveRemaining);

		return dest;
	}

	void GetDestination() {
		m_bCalculatingDestination = true;

		int attempts = 0;
		while (!m_bDestinationIsSet && attempts < 20) {
			m_CurrentDestination = GetExploreDestination();
			if (m_Entity.EvaluatePath(m_CurrentDestination)) {
				m_bDestinationIsSet = true;
			}
			attempts++;
		}

		m_bCalculatingDestination = false;
	}
}
