using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum BrainEvent {
	Finished = 0,
	None = 1
}

/// <summary>
/// This is the parent goal, utilized as the brain's thought process of an 
/// entity/unit which selects and/or executes subprocesses/goals.
/// </summary>
public class GoalThink : CompositeGoal {
	private List<GoalEvaluator> m_Evaluators = new List<GoalEvaluator>();
	private bool hasMoved = false;
	private bool hasAttacked = false;
	public bool HasMoved {
		get { return hasMoved; }
		set { hasMoved = value; }
	}
	public bool HasAttacked {
		get { return hasAttacked; }
		set { hasAttacked = value; }
	}

	bool justActivated = false;

	public delegate void BrainEventHandler (BrainEvent e);
	public event BrainEventHandler OnEvent;

	public GoalThink(Entity unit):base(unit, GoalType.Brain) {
		// applying our biases based on unit personality
		float HealthBias = m_Entity.Personality.healthBias;
		float ExploreBias = m_Entity.Personality.exploreBias;
		float HideBias = m_Entity.Personality.hideBias;
		float AttackBias = m_Entity.Personality.attackBias;
		float ReloadBias = m_Entity.Personality.reloadBias;

		// create evalutor objects
		m_Evaluators.Add (new HealthEvaluator(HealthBias));
		m_Evaluators.Add (new ExploreEvaluator(ExploreBias));
		m_Evaluators.Add (new AttackTargetEvaluator(AttackBias));
		m_Evaluators.Add (new HideEvaluator(HideBias));
		m_Evaluators.Add (new ReloadEvaluator(ReloadBias));
		m_Evaluators.Add(new DermalArmorEvaluator(HealthBias));
	}

	public override void Activate ()
	{
		justActivated = true;

		if (!m_Entity.isPossessed) {
			Arbitrate();
			GameManager.OnEntityEvent += HandleEntityEvent;
		}

		m_iStatus = (int)Status.Running;
	}

	public override int Process ()
	{
		ActivateIfInactive();

		int subgoalStatus = ProcessSubgoals();

		// if our unit is no longer selected end the turn
		if (!m_Entity.isSelected) {
			m_iStatus = (int)Status.Inactive;
			OnEvent(BrainEvent.Finished);
		}

		// if our unit is finished with their goals complete the turn
		if (subgoalStatus == (int)Status.Success || subgoalStatus == (int)Status.Failed) {
			if (m_Subgoals.Count > 0) {
				//Debug.Log ("[Brain] " + m_Subgoals[0].ToString() + " has " + ((Status)subgoalStatus).ToString());

				if (m_Subgoals[0].Type == GoalType.Attack) {
					if (subgoalStatus == (int)Status.Success ||
						m_Entity.Team.teamType == TeamType.AI) HasAttacked = true;
				} else if (m_Subgoals[0].Type == GoalType.Move) {
					AddGoal_TakeCover();
					QueueGoal_SelectTarget();
					if (subgoalStatus == (int)Status.Success ||
						m_Entity.Team.teamType == TeamType.AI) HasMoved = true;
				}
			}

			if (m_Subgoals.Count < 2 && (!HasMoved || !HasAttacked) && !m_Entity.isPossessed)
				Arbitrate();

			if (m_Subgoals.Count < 1) {
				m_iStatus = subgoalStatus;
				//Debug.Log ("Brain is finished with all goals.", m_Entity);
				
				OnEvent(BrainEvent.Finished);
			}
		}

		return m_iStatus;
	}

	public override void Terminate ()
	{
//		Debug.Log ("Terminating Brain.");
		base.Terminate ();

		GameManager.OnEntityEvent -= HandleEntityEvent;
	}

	/// <summary>
	/// Iterates through each goal option to determine which one has the highest desirability.
	/// </summary>
	public void Arbitrate() {
		float best = 0f;
		GoalEvaluator MostDesirable = null;

		// first things first, check for any new targets
		if (justActivated) {
			AddGoal_SelectTarget();
			justActivated = false;
			return;
		}

		// cycle through evaluators to see which has highest score
		for (int i = 0; i < m_Evaluators.Count; i++) {
			float desirability = m_Evaluators[i].CalculateDesirability(m_Entity);

			if (desirability >= best) {
				best = desirability;
				MostDesirable = m_Evaluators[i];
			}
		}

		if (MostDesirable == null || best == 0f) {
			//Debug.Log ("[GoalThink::Arbitrate] No evaluator selected, we're either done or broke.");
			return;
		}

		MostDesirable.SetGoal(m_Entity);
	}

	public bool notPresent (System.Type GoalType) {
		// checks to see if a goal is already at the front of our goals
		if (m_Subgoals.Count > 0) {
			return m_Subgoals[0].GetType() != GoalType;
		}

		return true;
	}

	#region Add/Queue Goal Methods
	public void AddGoal_MoveToPosition (Vector3 pos) {
		// immediately move to a new position
		AddSubgoal(new GoalMoveToPosition(m_Entity, pos));
	}

	public void AddGoal_Explore () {
		// used as a wander behaviour
		AddSubgoal (new GoalExplore(m_Entity));
	}

	public void AddGoal_GetItem(System.Type ItemType) {
		// used to recover an objective item
		/*if (notPresent(ItemTypeToGoalType(ItemType))) {
			RemoveAllSubgoals();
			AddSubgoal (new GoalGetItem(m_Entity, ItemType));
		}*/
	}

	public void AddGoal_AttackTarget() {
		// used to determine method of attack and execute
		if (notPresent(typeof(GoalAttackTarget))) {
			AddSubgoal (new GoalAttackTarget(m_Entity));
		}
	}
	public void QueueGoal_AttackTarget() {
		if (notPresent(typeof(GoalAttackTarget))) {
			QueueSubgoal(new GoalAttackTarget(m_Entity));
		}
	}

	public void AddGoal_Attack() {
		if (notPresent (typeof(GoalAttack))) {
			AddSubgoal(new GoalAttack(m_Entity));
		}
	}

	public void AddGoal_Hide() {
		// used to move into cover
		if (notPresent(typeof(GoalHide))) {
			//RemoveAllSubgoals();
			AddSubgoal(new GoalHide(m_Entity));
		}
	}

	public void AddGoal_SelectTarget() {
		// used to find a target
		if (notPresent(typeof(GoalSelectTarget))) {
			AddSubgoal(new GoalSelectTarget(m_Entity));
		}
	}
	public void AddGoal_SelectTarget(Entity target) {
		if (notPresent(typeof(GoalSelectTarget))) {
			AddSubgoal(new GoalSelectTarget(m_Entity, target));
		}
	}
	public void QueueGoal_SelectTarget() {
		// can be used to search for a target after a move
		QueueSubgoal(new GoalSelectTarget(m_Entity));
	}

	public void QueueGoal_MoveToPosition (Vector3 pos) {
		// can be used to queue multiple move destinations
		QueueSubgoal (new GoalMoveToPosition(m_Entity, pos));
	}

	public void QueueGoal_ReportTarget() {
		// let our teammates know our target's position
		QueueSubgoal (new GoalReportTarget(m_Entity));
	}

	public void AddGoal_TakeCover() {
		AddSubgoal(new GoalTakeCover(m_Entity));
	}
	public void QueueGoal_TakeCover() {
		QueueSubgoal(new GoalTakeCover(m_Entity));
	}

	public void AddGoal_Reload() {
		AddSubgoal (new GoalReload(m_Entity));
	}
	public void QueueGoal_Attack() {
		QueueSubgoal(new GoalAttack(m_Entity));
	}

	public void AddGoal_UseSpecial() {
		AddSubgoal(new GoalUseSpecial(m_Entity));
	}
	#endregion

	void HandleEntityEvent(EntityEvent e) {
		
	}
}
