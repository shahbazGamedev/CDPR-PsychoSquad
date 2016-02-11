using UnityEngine;
using System.Collections;

/// <summary>
/// Goal to hide unit from enemies and/or take cover.
/// </summary>
public class GoalHide : CompositeGoal {
	Vector3 targetPos = Vector3.zero;
	bool m_bDestinationSet = false;

	public GoalHide(Entity unit):base(unit, GoalType.Move) {}
	
	public override void Activate ()
	{
		targetPos = FindBestCover();

		if (!m_Entity.Agent.enabled) m_Entity.EnableAgent();
		m_iStatus = (int)Status.Running;
	}
	
	public override int Process ()
	{
		ActivateIfInactive();

		if (!m_Entity.Agent.enabled) {
			m_iStatus = (int)Status.Running;
		} else if (!m_bDestinationSet) {
			if (targetPos != Vector3.zero) {
				if (m_Entity.EvaluatePath(targetPos)) {
					AddSubgoal(new GoalSeekToPosition(m_Entity, targetPos));
				} else {
					m_iStatus = (int)Status.Failed;
				}
			} else {
				// flee in the opposite direction if possible
				Vector3 dir = -m_Entity.tr.forward;
				if (m_Entity.TargetSystem.isTargetPresent) {
					dir = (m_Entity.Position-m_Entity.TargetSystem.TargetPosition).normalized;
				} else {
					Vector3 tp = GameManager.TeamCenterPosition(m_Entity);
					dir = (tp-m_Entity.Position).normalized;
				}

				targetPos = m_Entity.Position + (dir*m_Entity.MoveRemaining);

				if (m_Entity.EvaluatePath(targetPos)) {
					AddSubgoal(new GoalSeekToPosition(m_Entity, targetPos));
				} else {
					m_iStatus = (int)Status.Failed;
				}
			}
			m_bDestinationSet = true;
		} else {
			m_iStatus = ProcessSubgoals();

			if (m_Subgoals.Count > 0) {
				m_iStatus = (int)Status.Running;
			}
		}

		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}

	Vector3 FindBestCover() {
		CoverPoint coverPoint = null;

		// if we have a target, find cover near it, otherwise, find cover near ourselves
		if (m_Entity.TargetSystem.isTargetPresent) {
			coverPoint = CoverManager.GetClosestCoverPoint(m_Entity.TargetSystem.TargetPosition);
		} else {
			coverPoint = CoverManager.GetClosestCoverPoint(m_Entity.Position);
		}

		if (coverPoint == null || (coverPoint.Position-m_Entity.Position).sqrMagnitude > (m_Entity.MoveRemaining*m_Entity.MoveRemaining))
			return Vector3.zero;
		
		return coverPoint.Position;
	}
}
