using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Goal to alert friendly units to a discovered enemy target.
/// </summary>
public class GoalReportTarget : Goal {
	List<Entity> teammates = null;

	//ctor
	public GoalReportTarget (Entity unit):base(unit, GoalType.Misc) {}
	
	public override void Activate ()
	{
		teammates = m_Entity.Team.Entities;

		m_iStatus = (int)Status.Running;
	}
	
	public override int Process ()
	{
		ActivateIfInactive();
		
		if (teammates != null && m_Entity.TargetSystem.isTargetPresent) {
			foreach (Entity u in teammates) {
				if (u != m_Entity) {
					u.Senses.UpdateMemory(m_Entity.TargetSystem.target);
				}
			}

			m_iStatus = (int)Status.Success;
		} else {
			m_iStatus = (int)Status.Failed;
		}

		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}
}
