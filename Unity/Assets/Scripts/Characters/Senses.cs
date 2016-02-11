using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles or confirms visual/auditory responses.
/// </summary>
public class Senses {
	public enum MemoryType {
		Sight = 0,
		Sound
	}

	public class MemoryData {
		public GameObject entity;
		public Entity unitEntity;
		public Vector3 position;
		public float time;
		public MemoryType memType;

		public MemoryData(){}
		public MemoryData (GameObject go, Vector3 pos, float t, MemoryType mt) {
			entity = go;
			position = pos;
			time = t;
			memType = mt;

			unitEntity = go.GetComponent<Entity>();
		}
	};

	Entity unit = null;
	float sightRange = 20f;
	float hearingRange = 50f;
	List<MemoryData> memory = null;
	const float forgetTime = 600f;

	// ctors
	public Senses() {}
	public Senses(Entity u, float sightDist, float hearDist) {
		unit = u;
		sightRange = sightDist;
		hearingRange = hearDist;
		memory = new List<MemoryData>();
	}


	#region Public Methods
	public bool CanHear (Vector3 pos) {
		return Toolbox.DistanceCheck(Vector3.zero, pos, hearingRange);
	}

	public bool CanSee (GameObject go) {
		if (go == null) return false;

		Entity e = go.GetComponent<Entity>();
		if (e != null) {
			StealthComponent comp = e.GetComponent<StealthComponent>();
			if (comp != null && comp.IsEnabled) {
				//Debug.Log("[Senses] : " + go.name + " has stealth activated.", unit);
				return false;
			}
		}

		// target is out of our sight range
		if (!Toolbox.DistanceCheck(unit.Position, go.transform.position, sightRange)) {
			//Debug.Log ("[Senses] : " + go.name + " out of sight range.", unit);
			return false;
		}

		// LOS check
		if (!LOS (unit.eyesTransform.position, go.transform)) {
			//Debug.Log ("[Senses] : Failed eyeline LOS check to " + go.name + ".", unit);
			return false;
		}

		return true;
	}

	/// <summary>
	/// Determines whether this instance can see the target from the specified position.
	/// </summary>
	/// <returns><c>true</c> if this instance can see from the specified pos go; otherwise, <c>false</c>.</returns>
	/// <param name="pos">Position.</param>
	/// <param name="go">Go.</param>
	public bool CanSeeFrom (Vector3 pos, GameObject go) {
		if (go == null) return false;

		if (!Toolbox.DistanceCheck(unit.Position, go.transform.position, sightRange))
			return false;

		if (!LOS (pos + (Vector3.up*unit.eyesTransform.position.y), go.transform))
			return false;

		return true;
	}

	public Vector3 GetLastRecordedPosition(GameObject go) {
		CheckForMemoryLoss();

		Vector3 pos = Vector3.zero;
		if (go == null) return pos;

		for (int i = 0; i < memory.Count; i++) {
			if (memory[i].entity == go) {
				pos = memory[i].position;
				//Debug.Log ("[Senses] : Last remembered position for " + go.name + ": " + pos.ToString(), unit);
				break;
			}
		}

		return pos;
	}

	public GameObject GetNearestEnemyFromMemory() {
		GameObject go = null;
		float d = 0f, dist = float.MaxValue;

		CheckForMemoryLoss();
		
		for (int i = 0; i < memory.Count; i++) {
			if ((d = (unit.Position-memory[i].position).sqrMagnitude) < dist) {
				go = memory[i].entity;
				dist = d;
				break;
			}
		}
		
		return go;
	}

	public Vector3 GetNearestEnemyPositionFromMemory () {
		Vector3 pos = Vector3.zero;
		float d = 0f, dist = float.MaxValue;

		CheckForMemoryLoss();

		for (int i = 0; i < memory.Count; i++) {
			if ((d = (unit.Position-memory[i].position).sqrMagnitude) < dist) {
				pos = memory[i].position;
				dist = d;
				break;
			}
		}

		return pos;
	}

	public void UpdateMemory(GameObject go, Vector3 pos, float t, MemoryType mt = MemoryType.Sight) {
		bool set = false;
		MemoryData md = new MemoryData(go, pos, t, mt);

		for (int i = 0; i < memory.Count; i++) {
			if (memory[i].entity == go) {
				memory[i] = md;
				set = true;
				break;
			}
		}

		if (!set) {
			memory.Add (md);
		}

		CheckForMemoryLoss();
	}

	public void UpdateMemory(GameObject go) {
		if (go == null) return;
		UpdateMemory (go, go.transform.position, Time.time);
	}

	public void RemoveFromMemory(GameObject go) {
		int ndx = -1;
		for (int i = 0; i < memory.Count; i++) {
			if (memory[i].entity == go) {
				ndx = i;
				break;
			}
		}

		if (ndx >= 0) memory.RemoveAt(ndx);
	}
	#endregion


	#region Helpers
	// we need to cycle through iterations of point checks to see if our
	// unit can see a part of the target game object
	bool LOS (Vector3 origin, Transform target) {
		Vector3 targetPos = target.position;

		RaycastHit hit;
		// base position is at their feet
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		CapsuleCollider cc = target.GetComponentInChildren<Collider>() as CapsuleCollider;
		if (cc == null) return false;
		else if (target != cc.transform) target = cc.transform;

		// look at the top of the collider
		targetPos = target.position + (Vector3.up*((cc.height)-0.01f));

		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		// look at the chest of the collider
		targetPos = target.position + (Vector3.up*(cc.height*0.75f));
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		// look at the waist of the collider
		targetPos = target.position + (Vector3.up*(cc.height*0.5f));
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		// look at the knees of the collider
		targetPos = target.position + (Vector3.up*(cc.height*0.25f));
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		// look at the right arm/hand/shoulder
		targetPos = target.position + (target.right*((cc.radius)-0.01f) + (Vector3.up*(cc.height*0.5f)));
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		// look at the left arm/hand/shoulder
		targetPos = target.position + (target.right*(-(cc.radius)+0.01f)) + (Vector3.up*(cc.height*0.5f));
		if (Physics.Raycast (origin, (targetPos-origin), out hit, sightRange*2f)) {
			if (hit.transform == target)
				return true;
		}

		return false;
	}

	void CheckForMemoryLoss() {
		memory.RemoveAll (obj => obj == null || obj.entity == null || (obj.unitEntity != null && !obj.unitEntity.isAlive) || (Time.time-obj.time) >= forgetTime);
	}
	#endregion
}
