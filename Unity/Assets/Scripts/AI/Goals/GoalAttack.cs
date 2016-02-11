using UnityEngine;
using System.Collections;

/// <summary>
/// Atomic goal to perform a single attack.
/// </summary>
public class GoalAttack : Goal {
	bool firstRun = true;
	float fireAnimationTime = 1f;
	float elapsedTime = 0f;
	WeaponInterface weap;

	public GoalAttack(Entity unit):base(unit, GoalType.Attack) {}
	
	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;
		m_Entity.isAttacking = true;
		weap = m_Entity.GetComponentInChildren<WeaponInterface>();
				
		// possible for unit's target to die whilst goal is active
		if (!m_Entity.TargetSystem.isTargetPresent || !m_Entity.TargetSystem.isTargetShootable || weap == null) {
			Debug.Log ("[GoalAttack] Target present: " + m_Entity.TargetSystem.isTargetPresent.ToString() + " or shootable: " + m_Entity.TargetSystem.isTargetShootable.ToString() + " or null weapon.");
			m_iStatus = (int)Status.Failed;
		}
	}
	
	public override int Process ()
	{
		ActivateIfInactive();
		if (m_iStatus == (int)Status.Failed) {
			return m_iStatus;
		}

		if (firstRun) {
			if (!m_Entity.TargetSystem.isTargetPresent || !m_Entity.TargetSystem.isTargetShootable) {
				Debug.Log ("[GoalAttack] Failed in loop! Target present: " + m_Entity.TargetSystem.isTargetPresent.ToString() + " or shootable: " + m_Entity.TargetSystem.isTargetShootable.ToString());
				m_iStatus = (int)Status.Failed;
			} else {
				// perform the actual attack
				m_Entity.Weapon.Fire();
				m_Entity.Brain.HasAttacked = true;
				m_Entity.Attack();
				m_Entity.tr.LookAt(m_Entity.TargetSystem.TargetPosition);
				weap.Fire(m_Entity.TargetSystem.target.transform, fireAnimationTime);
				m_iStatus = (int)Status.Success;
			}
		} else {
			elapsedTime += Time.deltaTime;
			m_iStatus = (int)Status.Running;

			m_Entity.tr.LookAt(m_Entity.TargetSystem.TargetPosition);

			if (elapsedTime >= fireAnimationTime) {
				m_iStatus = (int)Status.Success;
			}
		}

		if (firstRun && m_iStatus != (int)Status.Failed) {
			firstRun = false;
			m_iStatus = (int)Status.Running;
		}
		
		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_Entity.isAttacking = false;
		m_iStatus = (int)Status.Success;
	}
}
