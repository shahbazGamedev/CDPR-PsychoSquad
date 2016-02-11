using UnityEngine;
using System.Collections;

/// <summary>
/// Attack target evaluator.
/// </summary>
public class AttackTargetEvaluator : GoalEvaluator {

	public AttackTargetEvaluator(float bias):base(bias) {}

	public override float CalculateDesirability (Entity unit)
	{
		float desirability = 0f;

		// no sense in checking if there's no target present
		if (unit.TargetSystem.isTargetPresent && !unit.Brain.HasAttacked) {
			const float tweaker = 1f;

			desirability = tweaker * (unit.Health*0.01f) * (unit.Weapon.GetWeaponStrength()*0.1f);

			desirability *= m_fCharacterBias;
		}

		return desirability;
	}

	public override void SetGoal (Entity unit)
	{
		unit.Brain.AddGoal_AttackTarget();
	}
}
