using UnityEngine;
using System.Collections;

/// <summary>
/// Explore evaluator.
/// </summary>
public class ExploreEvaluator : GoalEvaluator {

	public ExploreEvaluator (float bias):base(bias) {}

	public override float CalculateDesirability (Entity unit)
	{
		float desirability = 1f;

		// no need to explore while we've got someone to kill
		if (unit.TargetSystem.isTargetPresent && unit.TargetSystem.isTargetShootable)
			desirability = 0f;
		else if (unit.Brain.HasMoved)
			desirability = 0f;

		desirability *= m_fCharacterBias;

		return desirability;
	}

	public override void SetGoal (Entity unit)
	{
		unit.Brain.AddGoal_Explore();
	}
}
