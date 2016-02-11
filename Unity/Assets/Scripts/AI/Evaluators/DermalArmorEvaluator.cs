using UnityEngine;
using System.Collections;

/// <summary>
/// Evaluator for dermal armor special ability.
/// </summary>
public class DermalArmorEvaluator : GoalEvaluator {

	public DermalArmorEvaluator(float bias):base(bias) {}

	public override float CalculateDesirability (Entity unit)
	{
		float desirability = 0f;

		// reloading counts as an attack action, and we should wait until we're in an ideal position
		// before determining if we should reload
		if (unit.AbilityType != AbilityType.Armor || !unit.Ability.isAvailable) {
			return desirability;
		}

		desirability = Mathf.Clamp01(1f-(unit.Health/unit.MaxHealth));
		desirability *= m_fCharacterBias;

		return desirability;
	}

	public override void SetGoal (Entity unit)
	{
		unit.Brain.AddGoal_UseSpecial();
	}
}
