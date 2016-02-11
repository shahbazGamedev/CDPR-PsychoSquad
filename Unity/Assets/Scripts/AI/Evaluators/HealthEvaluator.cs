using UnityEngine;
using System.Collections;

/// <summary>
/// Health evaluator.
/// </summary>
public class HealthEvaluator : GoalEvaluator {

	public HealthEvaluator (float bias):base(bias) {}

	public override float CalculateDesirability (Entity unit)
	{
		float tweaker = 0.2f;
		float desirability = tweaker * (1-(unit.Health*0.01f));
		desirability = Mathf.Clamp01 (desirability);
		desirability *= m_fCharacterBias;

		if (unit.Brain.HasMoved) desirability = 0f;

		return desirability;
	}

	public override void SetGoal (Entity unit)
	{
		// get to nearby a teammate who can heal

		// TODO fill this out properly
		unit.Brain.AddGoal_Hide();

	}
}
