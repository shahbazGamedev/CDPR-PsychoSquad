using UnityEngine;
using System.Collections;

/// <summary>
/// Target system confirms whether a target can be attacked.
/// </summary>
public class TargetSystem {
	Entity unit = null;
	public GameObject target = null;

	public TargetSystem() {}
	public TargetSystem(Entity u) {
		unit = u;
	}

	bool CanShootTarget() {
		// check if target is in our line of sight
		if (!unit.Senses.CanSee(target)) {
			//Debug.Log ("[TargetSystem] : Can't see target.");
			return false;
		}

		// check if our weapon can hit them
		if (!Toolbox.DistanceCheck(unit.Position, target.transform.position, unit.Weapon.GetWeaponRange())) {
			//Debug.Log ("[TargetSystem] : Out of weapon range.");
			return false;
		}

		// check if our weapon has ammo
		if (!unit.Weapon.HasAmmoLoaded()) {
			//Debug.Log ("[TargetSystem] : Out of ammo.");
			return false;
		}

		return true;
	}

	public void SetTarget(GameObject targetGO) {
		target = targetGO;
	}

	bool ValidTarget() {
		if (target != null) {
			Entity u = target.GetComponent<Entity>();
			if (u != null && u.isAlive) {
				return true;
			}
		}

		return false;
	}

	public Vector3 TargetPosition {
		get { return (target != null ? target.transform.position : Vector3.zero); }
	}
	public bool isTargetPresent {
		get { return ValidTarget(); }
	}
	public bool isTargetShootable {
		// perform range, LOS checks, etc here
		get { return CanShootTarget(); }
	}
	public bool isTargetVisible {
		get { return unit.Senses.CanSee(target); }
	}
}
