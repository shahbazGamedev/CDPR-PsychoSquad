using UnityEngine;
using System.Collections;

/// <summary>
/// Goal to move unit to a position.
/// </summary>
public class GoalMoveToPosition : CompositeGoal {
	private Vector3 m_vDestination;

	public GoalMoveToPosition(Entity unit, Vector3 pos):base(unit, GoalType.Move) {
		m_vDestination = pos;
	}

	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		RemoveAllSubgoals();

		AddSubgoal (new GoalSeekToPosition(m_Entity, m_vDestination));
	}

	public override int Process ()
	{
		ActivateIfInactive();

		m_iStatus = ProcessSubgoals();

		//ReactivateIfFailed();

		return m_iStatus;
	}

	public override bool HandleMessage (string msg)
	{
		bool bHandled = ForwardMessageToFrontMostSubgoal(msg);

		if (!bHandled) {
			switch (msg) {
			case "PathReady":
				RemoveAllSubgoals();
				return true;
			case "NoPathAvailable":
				m_iStatus = (int)Status.Failed;
				return true;
			default:
				return false;
			}
		}

		return true;
	}
}
