using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to make GameObject a permanent object that is not destroyed between scene loads.
/// </summary>
public class PermanentObject : MonoBehaviour {
	void Awake() {
		DontDestroyOnLoad(gameObject);
	}
}
