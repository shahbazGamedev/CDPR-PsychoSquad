using UnityEngine;
using System.Collections;

/// <summary>
/// Goal designed to move a unit from cover to a clear shot (without using movement).
/// </summary>
public class GoalStepFromCover : CompositeGoal {
	Vector3 origin = Vector3.zero;
	Vector3 dest = Vector3.zero;

	public GoalStepFromCover (Entity unit) : base(unit, GoalType.Misc) {}

	public override void Activate () {
		if (m_Entity.TargetSystem.isTargetPresent && m_Entity.TargetSystem.isTargetShootable) {
			m_iStatus = (int)Status.Success;
			return;
		}

		if (m_Entity.Cover == null || m_Entity.Cover.CalculateTotalCover() <= 0f) {
			m_iStatus = (int)Status.Failed;
			return;
		}

		origin = m_Entity.Position;
		dest = GetSidestepPos();

		if (dest != Vector3.zero) {
			AddSubgoal(new GoalSeekToPosition(m_Entity, dest));
		}
	}

	public override int Process() {
		ActivateIfInactive();

		m_iStatus = ProcessSubgoals();

		// check to see if we're at our sidestep destination
		if (m_iStatus != (int)Status.Running && Toolbox.DistanceCheck(dest, m_Entity.Position, 1f)) {
			if (m_Entity.Brain.HasAttacked) {
				// sidestepped, attacked, back to original pos
				AddSubgoal(new GoalSeekToPosition(m_Entity, origin));
			} else {
				// sidestepped, attack
				AddSubgoal(new GoalAttack(m_Entity));
			}
			m_iStatus = (int)Status.Running;
		}

		return m_iStatus;
	}


	Vector3 GetSidestepPos() {
		Vector3 pos = Vector3.zero;

		// TODO: finish this
		// get the cover object's relative position
		// we can assume cover exists at this point, we just need what direction to move
		// perhaps iterate through m_Entity.Cover.status to determine which combination of directions has cover, should
		// be able to figure it out based on which direct raycast has cover and which angled ones do not


		// can we see the target from predicted pos?
		if (m_Entity.Senses.CanSeeFrom(pos, m_Entity.TargetSystem.target)) {
			return pos;
		}

		return Vector3.zero;
	}
}
