using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Cover object with individual cover points.
/// </summary>
public class CoverObject : MonoBehaviour {
	// tracking vars
	[HideInInspector] public List<CoverPoint> coverPoints = new List<CoverPoint>();


	// Use this for initialization
	void Start () {
		CoverPoint[] cps = GetComponentsInChildren<CoverPoint>();
		foreach (CoverPoint cp in cps) {
			coverPoints.Add (cp);
		}
	}
}
