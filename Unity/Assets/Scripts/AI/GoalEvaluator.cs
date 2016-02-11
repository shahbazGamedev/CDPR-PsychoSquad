using UnityEngine;
using System.Collections;

/// <summary>
/// Goal evaluator class that defines an interface for objects that are able to evaluate
/// the desirability of a specific strategy level goal.
/// </summary>
public class GoalEvaluator {
	/// <summary>
	/// When the desirability score for a goal has been evaluated it is multiplied by this
	/// value. It can be used to create units with preferences based upon their personality.
	/// </summary>
	protected float m_fCharacterBias;

	public GoalEvaluator() {
		m_fCharacterBias = 1f;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GoalEvaluator"/> class.
	/// </summary>
	/// <param name="characterBias">Character bias.</param>
	public GoalEvaluator (float characterBias) {
		m_fCharacterBias = characterBias;
	}

	/// <summary>
	/// Returns a value between 0-1 representing the desirability of the strategy
	/// the concrete subclass represents.
	/// </summary>
	/// <returns>The desirability.</returns>
	/// <param name="unit">Entity.</param>
	public virtual float CalculateDesirability(Entity unit) {	return 0f; }

	/// <summary>
	/// Adds the goal to the given unit's brain.
	/// </summary>
	/// <param name="unit">Entity.</param>
	public virtual void SetGoal (Entity unit) {}
}
