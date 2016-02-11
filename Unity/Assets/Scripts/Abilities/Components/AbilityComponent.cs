using UnityEngine;
using System.Collections;

/// <summary>
/// Ability components are attached to units for discovery by game mechanics.
/// </summary>
public class AbilityComponent : MonoBehaviour {
	protected bool isEnabled = false;

	public virtual void Init() {
		Enable();
	}

	public virtual void Enable(bool enable = true) {
		IsEnabled = enable;
	}

	public virtual void OnDestroy() {
		Enable(false);
	}

	public bool IsEnabled {
		get { return isEnabled; }
		set { 
			if (!value) {
				Destroy(this);
			} else {
				isEnabled = value;
			}}
	}
}

public class StealthComponent : AbilityComponent {
	private Material mat;
	private MeshRenderer mr;

	public override void Enable(bool enable = true) {
		if (IsAssigned()) {
			Color c = mat.color;
			c.a = (enable ? 0.25f : 1f);
			mat.color = c;
			mr.material = mat;
		}

		base.Enable(enable);
	}

	private bool IsAssigned() {
		if (mr == null || mat == null) {
			mr = GetComponentInChildren<MeshRenderer>();
			mat = new Material(mr.material);
		}

		return (mr != null && mat != null);
	}
}

public class ArmorComponent : AbilityComponent {
	private float dmgReduction = 0.5f;

	public float ArmorRating {
		get { return dmgReduction; }
	}
}

public class AccuracyComponent : AbilityComponent {
	private float accuracyBoost = 0.5f;

	public float AccuracyRating {
		get { return accuracyBoost; }
	}
}

/// <summary>
/// Attached to objects revealed from a scan.
/// </summary>
public class RevealedComponent : AbilityComponent {}