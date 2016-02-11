using UnityEngine;
using System.Collections;

/// <summary>
/// Parent goal for attacking a target.
/// </summary>
public class GoalAttackTarget : CompositeGoal {

	public GoalAttackTarget(Entity unit):base(unit, GoalType.Attack) {}

	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		RemoveAllSubgoals();

		// possible for unit's target to die whilst goal is active
		if (!m_Entity.TargetSystem.isTargetPresent) {
			m_iStatus = (int)Status.Failed;
			return;
		}

		// if the unit is able to shoot the target, select a
		// tactic to use while shooting
		if (m_Entity.TargetSystem.isTargetVisible) {
			if (!m_Entity.Brain.HasMoved) {
				// try to shoot from cover
				AddSubgoal (new GoalAttack(m_Entity));
				AddSubgoal (new GoalFindAttackCover(m_Entity));
			} else {
				// attack from here
				AddSubgoal (new GoalAttack(m_Entity));
			}
		} else if (!m_Entity.Brain.HasMoved){
			// if target not visible, track 'er down
			//AddSubgoal (new GoalAttack(m_Entity));
			AddSubgoal (new GoalHuntTarget(m_Entity));
		} else {
			//Debug.Log ("Target not shootable.");
			m_iStatus = (int)Status.Failed;
		}
	}

	public override int Process ()
	{
		ActivateIfInactive();

		m_iStatus = ProcessSubgoals();

		if (m_iStatus != (int)Status.Failed && m_Subgoals.Count > 0) {
			m_iStatus = (int)Status.Running;
		}

		return m_iStatus;
	}

	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}
}
