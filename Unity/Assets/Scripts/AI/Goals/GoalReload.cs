using UnityEngine;
using System.Collections;

/// <summary>
/// Atomic goal to perform a weapon reload.
/// </summary>
public class GoalReload : Goal {
	bool playingAnim = false;
	float animationPlayTime = 2f;
	float timeElapsed = 0f;

	//ctor
	public GoalReload (Entity unit):base(unit, GoalType.Attack) {}
	
	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		if (m_Entity.Brain.HasAttacked) return;

		if (m_Entity.Weapon.RoundsSpare() > 0) {
			m_Entity.Weapon.Reload();

			m_Entity.Reload();
			playingAnim = true;
			timeElapsed = 0f;
			m_Entity.Brain.HasAttacked = true;
		}
	}

	public override int Process ()
	{
		ActivateIfInactive();

		if (playingAnim && timeElapsed < animationPlayTime) {
			timeElapsed += Time.deltaTime;
			m_iStatus = (int)Status.Running;
		} else {
			m_iStatus = (int)Status.Success;
		}
		
		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}
}
