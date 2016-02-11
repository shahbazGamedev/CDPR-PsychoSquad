using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ranged group heal ability.
/// </summary>
public class HealAbility : AbilityBase {
	private float healthCharge = 20f;

	public HealAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public HealAbility(Entity e) {
		name = "Nanite Cloud";
		owner = e;
		duration = 1;
		cooldownDelay = 5;
		range = 7f;
		usesMove = false;
		usesAttack = true;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();

		// determine what friendly entities are in range and heal them
		for (int i = 0; i < GameManager.Entities.Length; i++) {
			if (GameManager.Entities[i].Team == owner.Team && GameManager.Entities[i].isAlive &&
				Toolbox.DistanceCheck(owner.Position, GameManager.Entities[i].Position, range)) {

				GameManager.Entities[i].TakeDamage(-healthCharge, owner.gameObject, false, false);
			}
		}
	}
}

/// <summary>
/// Ammo ability.
/// </summary>
public class AmmoAbility : AbilityBase {
	public AmmoAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public AmmoAbility(Entity e) {
		name = "Ammo Pack";
		owner = e;
		duration = 1;
		cooldownDelay = 5;
		range = 7f;
		usesMove = false;
		usesAttack = true;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();
	}
}

/// <summary>
/// Sprint ability.
/// </summary>
public class SprintAbility : AbilityBase {
	public SprintAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public SprintAbility(Entity e) {
		name = "Sprint";
		owner = e;
		duration = 1;
		cooldownDelay = 5;
		range = 7f;
		usesMove = false;
		usesAttack = true;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();
	}
}

/// <summary>
/// Reflex ability.
/// </summary>
public class ReflexAbility : AbilityBase {
	public ReflexAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public ReflexAbility(Entity e) {
		name = "Wired Reflexes";
		owner = e;
		duration = 1;
		cooldownDelay = 5;
		range = 0f;
		usesMove = false;
		usesAttack = true;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();
	}
}

/// <summary>
/// Stealth ability, makes invis to enemies and grants sneak bonus.
/// </summary>
public class StealthAbility : AbilityBase {
	public StealthAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public StealthAbility(Entity e) {
		name = "ThermOptic Camo";
		owner = e;
		duration = 2;
		cooldownDelay = 5;
		range = 0f;
		usesMove = false;
		usesAttack = false;
	}

	protected override void ActivateAbility () {
		base.ActivateAbility ();

		StealthComponent comp = owner.gameObject.AddComponent<StealthComponent>();
		if (comp != null) {
			comp.Init();
		}
	}

	protected override void DeactivateAbility() {
		StealthComponent comp = owner.gameObject.GetComponent<StealthComponent>();
		if (comp != null) {
			owner.RemoveComponent(comp);
		}
	}
}

/// <summary>
/// Ranged scan ability to uncover hidden enemies.
/// </summary>
public class ScanAbility : AbilityBase {
	public ScanAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public ScanAbility(Entity e) {
		name = "Scan";
		owner = e;
		duration = 1;
		cooldownDelay = 3;
		range = owner.SightRange*1.5f;
		usesMove = false;
		usesAttack = true;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();

		Entity[] e = GameManager.GetEnemiesInRange(owner, range);
		if (e != null) {
			for (int i = 0; i < e.Length; i++) {
				e[i].gameObject.AddComponent<RevealedComponent>();
			}
		}
	}

	protected override void DeactivateAbility ()
	{
		base.DeactivateAbility ();

		RevealedComponent comp;
		Entity[] e = GameManager.Entities;
		for (int i = 0; i < e.Length; i++) {
			if (e[i].Team != owner.Team) {
				if ((comp = e[i].GetComponent<RevealedComponent>()) != null) {
					e[i].RemoveComponent(comp);
				}
			}
		}
	}
}

/// <summary>
/// Armor ability for damage reduction.
/// </summary>
public class ArmorAbility : AbilityBase {
	public ArmorAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public ArmorAbility(Entity e) {
		name = "Dermal Armor";
		owner = e;
		duration = 1;
		cooldownDelay = 5;
		range = 0f;
		usesMove = false;
		usesAttack = false;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();

		ArmorComponent comp = owner.gameObject.AddComponent<ArmorComponent>();
		if (comp != null) {
			comp.Init();
		}
	}

	protected override void DeactivateAbility() {
		base.DeactivateAbility();

		ArmorComponent comp = owner.gameObject.GetComponent<ArmorComponent>();
		if (comp != null) {
			owner.RemoveComponent(comp);
		}
	}
}

/// <summary>
/// Accuracy ability, grants current round perfect accuracy.
/// </summary>
public class AccuracyAbility : AbilityBase {
	public AccuracyAbility() : this(null) {
		Debug.LogError(this.name + " is using an invalid constructor.");
	}
	public AccuracyAbility(Entity e) {
		name = "Smart-Link";
		owner = e;
		duration = 1;
		cooldownDelay = 4;
		range = 0f;
		usesMove = false;
		usesAttack = false;
	}

	protected override void ActivateAbility() {
		base.ActivateAbility();

		AccuracyComponent comp = owner.gameObject.AddComponent<AccuracyComponent>();
		if (comp != null) {
			comp.Init();
		}
	}

	protected override void DeactivateAbility() {
		base.DeactivateAbility();

		AccuracyComponent comp = owner.gameObject.GetComponent<AccuracyComponent>();
		if (comp != null) {
			owner.RemoveComponent(comp);
		}
	}
}