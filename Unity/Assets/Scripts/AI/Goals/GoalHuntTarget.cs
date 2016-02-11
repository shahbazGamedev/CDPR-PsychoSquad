using UnityEngine;
using System.Collections;

/// <summary>
/// Goal for unit to hunt a target that is no longer visible/attackable from their current position.
/// </summary>
public class GoalHuntTarget : CompositeGoal {
	private bool m_bDestinationSet = false;

	public GoalHuntTarget(Entity unit):base(unit, GoalType.Move) {}

	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		RemoveAllSubgoals();

		if (!m_Entity.Agent.enabled) m_Entity.EnableAgent();
	}

	public override int Process ()
	{
		if (!m_Entity.Agent.enabled) {
			m_iStatus = (int)Status.Running;
		} else if (!m_Entity.Brain.HasMoved && !m_bDestinationSet) {
			// if there's a desired target, pursue it.
			// if not, perhaps consider another nearby target to pursue.
			Vector3 lrp = Vector3.zero;

			// assuming a target is present, where did we last see them?
			if (m_Entity.TargetSystem.isTargetPresent)
				lrp = m_Entity.Senses.GetLastRecordedPosition(m_Entity.TargetSystem.target);

			// no memory of our target, let's try another from memory
			if (lrp == Vector3.zero) {
				// pick another target
				m_Entity.TargetSystem.SetTarget(m_Entity.Senses.GetNearestEnemyFromMemory());
				lrp = m_Entity.Senses.GetLastRecordedPosition(m_Entity.TargetSystem.target);
			}

			if (lrp == Vector3.zero || Toolbox.DistanceCheck(m_Entity.Position, lrp, 2f)) {
				// maybe we can find another target in our nearby surroundings
				AddSubgoal (new GoalSelectTarget(m_Entity));
			} else {
				// chase the bugger down
				// TODO restrict to moveDist
				if (!Toolbox.DistanceCheck(m_Entity.Position, lrp, m_Entity.MoveRemaining)) {
					Vector3 dir = (lrp-m_Entity.Position).normalized;
					lrp = m_Entity.Position + (dir*m_Entity.MoveRemaining);
				}

				if (m_Entity.EvaluatePath(lrp))
					AddSubgoal (new GoalSeekToPosition(m_Entity, lrp));
				else
					m_iStatus = (int)Status.Failed;
			}
			m_bDestinationSet = true;
		} else if (m_Entity.Brain.HasMoved) {
			// we've already moved, shouldn't be here
			m_iStatus = (int)Status.Success;
		} else {
			m_iStatus = ProcessSubgoals();

			if (m_Subgoals.Count > 0 && m_iStatus != (int)Status.Failed) {
				m_iStatus = (int)Status.Running;
			} else if (m_Entity.TargetSystem.isTargetPresent && m_Entity.TargetSystem.isTargetShootable) {
				m_iStatus = (int)Status.Success;
			} else if (m_Entity.TargetSystem.isTargetPresent) {
				// stop chasing, see if there's a better target to go after
				// or let explore find a new one
				m_Entity.Senses.RemoveFromMemory(m_Entity.TargetSystem.target);
				m_Entity.Brain.QueueGoal_SelectTarget();
				m_iStatus = (int)Status.Failed;
			}
		}

		return m_iStatus;
	}
}
