using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically destroys the oboject this is attached to after delay has expired.
/// </summary>
public class AutoDestroy : MonoBehaviour {
	public float delay = 1.1f;

	// Use this for initialization
	void Start () {
		StartCoroutine(DestroyTime(delay));
	}

	IEnumerator DestroyTime(float d) {
		yield return new WaitForSeconds(d);
		Destroy(gameObject);
	}
}
