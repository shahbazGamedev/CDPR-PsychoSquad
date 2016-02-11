using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Goal to find appropriate cover that is defensible while still being able to attack their target.
/// </summary>
public class GoalFindAttackCover : CompositeGoal {
	Vector3 targetPos = Vector3.zero;


	public GoalFindAttackCover(Entity unit):base(unit, GoalType.Attack) {}
	
	public override void Activate ()
	{
		RemoveAllSubgoals();

		m_iStatus = (int)Status.Running;

		Vector2 idealRange = m_Entity.Weapon.GetIdealRange();
		targetPos = FindBestCover(idealRange);
		
		if (targetPos != Vector3.zero) {
			AddSubgoal(new GoalSeekToPosition(m_Entity, targetPos));
		}
	}
	
	public override int Process ()
	{
		ActivateIfInactive();
		
		m_iStatus = ProcessSubgoals();

		if (m_Subgoals.Count > 0) {
			m_iStatus = (int)Status.Running;
		}
		
		return m_iStatus;
	}
	
	public override void Terminate ()
	{
		m_iStatus = (int)Status.Success;
	}

	Vector3 FindBestCover(Vector2 range) {
		CoverPoint coverPoint = null;

		// utilizing the weapon's best range qualities
		CoverPoint[] coverArray = CoverManager.GetPointsWithin(m_Entity.TargetSystem.TargetPosition, range);

		// reducing them to ones reachable
		if (coverArray != null && coverArray.Length > 0) {
			List<CoverPoint> cps = new List<CoverPoint>();
			for (int i = 0; i < coverArray.Length; i++) {
				if ((coverArray[i].Position-m_Entity.Position).sqrMagnitude <= m_Entity.MoveRemaining*m_Entity.MoveRemaining) {
					cps.Add (coverArray[i]);
				}
			}

			// find the one closest to ourselves
			if (cps.Count > 0) {
				float d = 0, dist = float.MaxValue;

				for (int i = 0; i < cps.Count; i++) {
					if ((d = (cps[i].Position-m_Entity.Position).sqrMagnitude) < dist) {
						dist = d;
						coverPoint = cps[i];
					}
				}
			}
		}

		CoverPoint coverPointA = null;
		CoverPoint coverPointB = null;
		
		// find cover near our target and find cover near ourselves
		if (coverPoint == null) {
			coverPointA = CoverManager.GetClosestPointBetween(m_Entity.Position, m_Entity.TargetSystem.TargetPosition);
		} else {
			coverPointA = coverPoint;
		}
		coverPointB = CoverManager.GetClosestCoverPoint(m_Entity.Position);

		// just in case
		if (coverPointA == null) return Vector3.zero;

		// determine which is better
		if (coverPointA != coverPointB) {
			if ((coverPointA.Position-m_Entity.TargetSystem.TargetPosition).sqrMagnitude <= m_Entity.Weapon.GetWeaponRangeSqrd()) {
				// A-OK (har). we check A first because it means we're likely staying closer to our team
				coverPoint = coverPointA;
			} else if ((coverPointB.Position-m_Entity.TargetSystem.TargetPosition).sqrMagnitude <= m_Entity.Weapon.GetWeaponRangeSqrd()) {
				// B-OK
				coverPoint = coverPointB;
			}
		} else if ((coverPointA.Position-m_Entity.TargetSystem.TargetPosition).sqrMagnitude <= m_Entity.Weapon.GetWeaponRangeSqrd()) {
			// still good to shoot here
			coverPoint = coverPointA;
		}
		
		if (coverPoint == null || (coverPoint.Position-m_Entity.Position).sqrMagnitude > (m_Entity.MoveRemaining*m_Entity.MoveRemaining))
			return Vector3.zero;
		
		return coverPoint.Position;
	}
}
