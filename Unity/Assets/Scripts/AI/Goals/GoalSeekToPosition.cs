using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Atomic goal for basic pathfinding navigation.
/// </summary>
public class GoalSeekToPosition : Goal {
	#region Setup Vars
	private Vector3 m_vPosition;
	private NavMeshPath path = null;
	#endregion

	//ctor
	public GoalSeekToPosition (Entity unit, Vector3 pos):base(unit, GoalType.Move) {
		m_vPosition = pos;
	}

	#region Goal Methods
	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;
		m_Entity.isMoving = true;
		if (!m_Entity.Agent.enabled) m_Entity.EnableAgent();

		// we'll need to update our cover after a move
		m_Entity.Brain.QueueGoal_TakeCover();
	}

	public override int Process ()
	{
		ActivateIfInactive();


		if (!m_Entity.Agent.enabled) {
			m_iStatus = (int)Status.Running;
		} else if (path == null) {
			path = new NavMeshPath();
			m_Entity.Agent.CalculatePath(m_vPosition, path);

			if (path.status == NavMeshPathStatus.PathComplete) {
				m_Entity.Agent.SetDestination(m_vPosition);
			} else {
				m_iStatus = (int)Status.Failed;
			}
		} else {
			if (path.status == NavMeshPathStatus.PathInvalid) {
				Debug.LogError("Unable to finish path to destination.", m_Entity);
				m_iStatus = (int)Status.Failed;
			} else if (m_Entity.Agent.hasPath && m_Entity.Agent.remainingDistance <= 0.1f) {
				m_iStatus = (int)Status.Success;
				m_Entity.Brain.HasMoved = true;
			} else {
				m_iStatus = (int)Status.Running;
			}
		}

		return m_iStatus;
	}

	public override void Terminate ()
	{
		m_Entity.isMoving = false;

		path.ClearCorners();
		path = null;

		m_Entity.SendEvent(null, EntityEventType.MoveComplete);

		m_iStatus = (int)Status.Success;
	}
	#endregion
}
