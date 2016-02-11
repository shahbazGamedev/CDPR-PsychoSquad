/// <summary>
/// Goal oriented behaviour design has been modified and repurposed for Unity C# from 
/// Mat Buckland's "Programming Game AI By Example" book, originally provided in C++ 
/// using a few helper libraries. 
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base Goal class, directly inherit from for an atomic goal.
/// </summary>
public class Goal {
	public enum Status {
		Inactive = 0,
		Running = 1,
		Success = 2,
		Failed = 3
	}

	public enum GoalType {
		Move = 0,
		Attack = 1,
		Misc = 2,
		Brain = 3
	}

	protected System.Type m_Type;
	protected Entity m_Entity;
	protected int m_iStatus;
	protected GoalType m_GoalType;

	/// <summary>
	/// Activates if inactive.
	/// </summary>
	protected void ActivateIfInactive() {
		if (isInactive) {
			Activate();
		}
	}

	/// <summary>
	/// Reactivates the goal if failed.
	/// </summary>
	protected void ReactivateIfFailed() {
		if (hasFailed) {
			m_iStatus = (int)Status.Inactive;
		}
	}

	public Goal() {
		Debug.LogError ("Do not create a goal without assigning a unit that's going to use it.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Goal"/> class.
	/// </summary>
	/// <param name="unit">Entity.</param>
	/// <param name="type">Type.</param>
	public Goal (Entity unit, GoalType type) {
		m_Entity = unit;
		m_GoalType = type;
		m_iStatus = (int)Status.Inactive;
		//m_Type = typeof();
	}

	public virtual void Activate() {}
	public virtual int Process() { return 0; }
	public virtual void Terminate() {}
	public virtual bool HandleMessage(string msg) { return false; }

	public virtual void AddSubgoal(Goal g) {
		Debug.LogError ("Do not add subgoals to atomic goals.");
	}
	public virtual void QueueSubgoal (Goal g) {
		Debug.LogError ("Do not add subgoals to atomic goals.");
	}


	public bool isComplete { get { return m_iStatus == (int)Status.Success; } }
	public bool isRunning { get { return m_iStatus == (int)Status.Running; } }
	public bool isInactive { get { return m_iStatus == (int)Status.Inactive; } }
	public bool hasFailed { get { return m_iStatus == (int)Status.Failed; } }
	public GoalType Type { get { return m_GoalType; } }


}

/// <summary>
/// Composite goals contain more than one atomic goal and self-standing logic.
/// </summary>
public class CompositeGoal : Goal {
	protected List<Goal> m_Subgoals = new List<Goal>();

	/// <summary>
	/// Removes any completed goals from the front of the subgoal list. 
	/// Then processes the next subgoal in the list if one is available.
	/// </summary>
	/// <returns>The subgoals.</returns>
	protected int ProcessSubgoals() {
		while (m_Subgoals.Count > 0 && (m_Subgoals[0].isComplete || m_Subgoals[0].hasFailed)) {
			m_Subgoals[0].Terminate();
			m_Subgoals.RemoveAt(0);
		}

		if (m_Subgoals.Count > 0) {

			// for some reason it denotes that process should always activate if inactive,
			// don't really like that
			if (m_Subgoals[0].isInactive) {
				m_Subgoals[0].Activate();
			}

			int StatusOfSubGoals = m_Subgoals[0].Process();

			if  (StatusOfSubGoals == (int)Status.Success && m_Subgoals.Count > 1) {
				return (int)Status.Running;
			}

			return StatusOfSubGoals;
		} else {
			return (int)Status.Success;
		}
	}

	/// <summary>
	/// Forwards the message to front most subgoal.
	/// </summary>
	/// <returns><c>true</c>, if message to front most subgoal was forwarded, <c>false</c> otherwise.</returns>
	/// <param name="msg">Message.</param>
	protected bool ForwardMessageToFrontMostSubgoal(string msg) {
		if (m_Subgoals.Count > 0) {
			return m_Subgoals[0].HandleMessage(msg);
		}

		return false;
	}

	public CompositeGoal() {
		Debug.LogError ("Do not create a goal without assigning a unit that's going to use it.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompositeGoal"/> class.
	/// </summary>
	/// <param name="unit">Entity.</param>
	/// <param name="type">Type.</param>
	public CompositeGoal (Entity unit, GoalType type):base(unit, type) {}

	public override void Terminate ()
	{
		RemoveAllSubgoals();
	}

	/// <summary>
	/// Adds the subgoal to the front of the list.
	/// </summary>
	/// <param name="g">The goal component.</param>
	public override void AddSubgoal(Goal g) {
//#if DEBUG
//		Debug.Log ("[Brain] " + m_Entity.name + " Added Subgoal: " + g.ToString(), m_Entity);
//#endif
		m_Subgoals.Insert(0, g);
	}

	/// <summary>
	/// Queues the subgoal at the end of the list.
	/// </summary>
	/// <param name="g">The goal component.</param>
	public override void QueueSubgoal (Goal g) {
//#if DEBUG
//		Debug.Log ("[Brain] " + m_Entity.name + " Queued Subgoal: " + g.ToString(), m_Entity);
//#endif
		m_Subgoals.Add (g);
	}

	/// <summary>
	/// Removes all subgoals.
	/// </summary>
	public void RemoveAllSubgoals() {
		for (int i = 0; i < m_Subgoals.Count; i++) {
			m_Subgoals[i].Terminate();
		}

		m_Subgoals.Clear();
	}
}
