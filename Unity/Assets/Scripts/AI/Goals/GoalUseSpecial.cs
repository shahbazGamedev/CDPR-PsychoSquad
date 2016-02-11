using UnityEngine;
using System.Collections;

/// <summary>
/// Atomic goal to use a special ability if possible.
/// </summary>
public class GoalUseSpecial : Goal {

	public GoalUseSpecial(Entity unit):base(unit, GoalType.Misc) {}

	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		if (!m_Entity.Ability.isAvailable) {
			m_iStatus = (int)Status.Failed;
		}
	}

	public override int Process ()
	{
		ActivateIfInactive();

		m_Entity.UseSpecial();
		m_iStatus = (m_Entity.Ability.inUse ? (int)Status.Success : (int)Status.Failed);

		return m_iStatus;
	}

	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}
}
