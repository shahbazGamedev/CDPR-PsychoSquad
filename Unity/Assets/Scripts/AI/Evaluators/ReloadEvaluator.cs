using UnityEngine;
using System.Collections;

/// <summary>
/// Reload evaluator.
/// </summary>
public class ReloadEvaluator : GoalEvaluator {
	
	public ReloadEvaluator(float bias):base(bias) {}
	
	public override float CalculateDesirability (Entity unit)
	{
		float desirability = 0f;
		const float tweaker = 0.2f;

		// reloading counts as an attack action, and we should wait until we're in an ideal position
		// before determining if we should reload
		if (unit.Brain.HasAttacked || !unit.Brain.HasMoved) return desirability;

		// we have zero ammo.. it's time to reload no matter what
		if (unit.Weapon.RoundsLoaded() < 1 && unit.Weapon.HasAmmoSpare()) {
			desirability = 1f;
		} else if (unit.Weapon.GetMagazineSize() > unit.Weapon.RoundsLoaded() && unit.Weapon.HasAmmoSpare()) {

			// if there's nobody to worry about, we should reload
			if (!unit.TargetSystem.isTargetPresent || 
			    (unit.Position-unit.Senses.GetNearestEnemyPositionFromMemory()).sqrMagnitude >= unit.SightRange) {
				desirability = 1f;
			} else {							
				desirability = (1f-(float)unit.Weapon.RoundsLoaded()/(float)unit.Weapon.GetMagazineSize())*tweaker;
				desirability *= m_fCharacterBias;
			}
		}
		
		return desirability;
	}
	
	public override void SetGoal (Entity unit)
	{
		unit.Brain.AddGoal_Reload();
	}
}