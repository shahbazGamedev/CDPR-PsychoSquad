using UnityEngine;
using System.Collections;

/// <summary>
/// Hide evaluator.
/// </summary>
public class HideEvaluator : GoalEvaluator {
	
	public HideEvaluator(float bias):base(bias) {}
	
	public override float CalculateDesirability (Entity unit)
	{
		float desirability = 0f;
		
		// can't hide from something if there's no target present
		if (unit.TargetSystem.isTargetPresent && !unit.Brain.HasMoved) {
			const float tweaker = 1f;

			// TODO check for if our unit is already hidden

			// comparing health and weapon strengths
			Entity target = unit.TargetSystem.target.GetComponent<Entity>();
			if (target != null) {
				bool targetHealthGreater = target.Health > unit.Health;
				desirability = tweaker * (targetHealthGreater ? (target.Health-unit.Health)*0.01f : 0f) * 
					((target.Weapon.GetWeaponStrength()-unit.Weapon.GetWeaponStrength())*0.01f);
				
				desirability *= m_fCharacterBias;
			}
		}
		
		return desirability;
	}
	
	public override void SetGoal (Entity unit)
	{
		unit.Brain.AddGoal_Hide();
	}
}