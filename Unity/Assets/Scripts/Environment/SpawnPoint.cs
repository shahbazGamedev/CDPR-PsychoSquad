using UnityEngine;
using System.Collections;

/// <summary>
/// Team spawn point parent.
/// </summary>
public class SpawnPoint : MonoBehaviour {
	public int team = -1;
	public Transform[] spawns;

	// Use this for initialization
	void Start () {
		foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
			r.enabled = false;
		}
	}

	public Vector3 GetSpawnPos (int ndx) {
		if (ndx >= spawns.Length) return Toolbox.InvalidV3;

		return spawns[ndx].position;
	}
}
