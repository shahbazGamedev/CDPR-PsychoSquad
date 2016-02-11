using UnityEngine;
using System.Collections;

/// <summary>
/// Obstruction class attached to objects obstructing camera view.
/// </summary>
public class Obstruction : MonoBehaviour {
	MeshRenderer[] mr;
	Material[] mat;

	// Use this for initialization
	void Start () {
		mr = GetComponentsInChildren<MeshRenderer>();
		mat = new Material[mr.Length];
		for (int i = 0; i < mr.Length; i++) {
			mat[i] = mr[i].sharedMaterial;
		}

		SetMaterial();
	}

	public void Remove() {
		SetMaterial(false);
		Destroy(this);
	}

	void SetMaterial(bool hide = true) {
		if (mr == null || mr.Length == 0) return;

		for (int i = 0; i < mr.Length; i++) {
			mr[i].material = (hide ? FollowMouse.HideMaterial : mat[i]);
		}
	}
}
