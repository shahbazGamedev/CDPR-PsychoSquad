using UnityEngine;
using System.Collections;

/// <summary>
/// Atomic goal that updates a unit's current cover.
/// </summary>
public class GoalTakeCover : Goal {
	const float m_fCoverRange = 2f;
	bool goodCover = false;

	//ctor
	public GoalTakeCover (Entity unit):base(unit, GoalType.Misc) {
		goodCover = false;
	}

	public override void Activate ()
	{
		if (m_Entity.Cover == null) {
			m_iStatus = (int)Status.Failed;
			return;
		}

		m_iStatus = (int)Status.Running;

		if (m_Entity.Cover.CalculateTotalCover() > 0f) goodCover = true;
	}
	
	public override int Process ()
	{
		ActivateIfInactive();
		
		if (goodCover) {
			// transition to crouch/cover animation
		} else {
			// remaining standing
		}

		m_iStatus = (int)Status.Success;
		
		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}
}
