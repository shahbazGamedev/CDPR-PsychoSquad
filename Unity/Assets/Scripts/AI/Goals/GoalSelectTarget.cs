using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Atomic goal that selects the current unit's best target.
/// </summary>
public class GoalSelectTarget : Goal {
	private Entity target;
	
	public GoalSelectTarget(Entity unit):base(unit, GoalType.Misc) {
		target = null;
	}
	public GoalSelectTarget(Entity unit, Entity tgt):base(unit, GoalType.Misc) {
		target = tgt;
	}
	
	public override void Activate ()
	{
		m_iStatus = (int)Status.Running;

		if (target == null) {
			target = GetBestEnemy();
		
			if (target == null) {
				//Debug.Log ("[GoalSelectTarget] : No targets found.");
				m_iStatus = (int)Status.Failed;
			} else {
				//Debug.Log ("[GoalSelectTarget] : " + m_Entity.name + " selected target of " + target.name);
				m_Entity.TargetSystem.SetTarget(target.gameObject);
				m_Entity.Senses.UpdateMemory(m_Entity.TargetSystem.target);
				m_Entity.Brain.QueueGoal_ReportTarget();
				m_iStatus = (int)Status.Success;
			}
		} else {
			m_Entity.TargetSystem.SetTarget(target.gameObject);
			m_Entity.Senses.UpdateMemory(m_Entity.TargetSystem.target);
			m_Entity.Brain.QueueGoal_ReportTarget();
			m_iStatus = (int)Status.Success;
		}
	}
	
	public override int Process ()
	{
		ActivateIfInactive();
		
		return m_iStatus;
	}
	
	Entity GetBestEnemy() {
		List<Entity> enemy = new List<Entity>();
		Entity e = null;
		
		// sorting into teams
		for (int i = 0; i < GameManager.Entities.Length; i++) {
			if (GameManager.Entities[i].Team != m_Entity.Team) {
				enemy.Add(GameManager.Entities[i]);
			}
		}

		// determine best fit
		float sqrRange = m_Entity.Weapon.GetWeaponRangeSqrd()*2f;	// multiplied by two for grid size
		float d = 0f, dist = sqrRange;
		for (int i = 0; i < enemy.Count; i++) {
			if (!enemy[i].isAlive || !m_Entity.Senses.CanSee(enemy[i].gameObject)) continue;

			d = ((enemy[i].Position-m_Entity.Position).sqrMagnitude);
			if ((d = ((enemy[i].Position-m_Entity.Position).sqrMagnitude)) <= dist) {
				/* TODO : evaluate surroundings, etc to pick best target
				 * if (e != null && (int)e.Senses.CoverStatus < (int)enemy[i].Senses.CoverStatus) {

				}*/
				dist = d;
				e = enemy[i];
			}
		}

		// try to remember someone we've seen recently
		if (e == null) {
			GameObject go = m_Entity.Senses.GetNearestEnemyFromMemory();
			if (go != null) {
				e = go.GetComponent<Entity>();
			}
		}
		
		return e;
	}
}
