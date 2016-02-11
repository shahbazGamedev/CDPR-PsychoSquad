using UnityEngine;
using System.Collections;

public enum CoverStatus {
	None = 0,
	Half,
	Full
}

/// <summary>
/// Individual cover point belonging to a CoverObject.
/// </summary>
public class CoverPoint : MonoBehaviour {
	public CoverStatus coverStatus = CoverStatus.Half;
	public bool isFilled = false;
	public GameObject filledBy;
	[HideInInspector]
	public Transform tr;

	void Start()
	{
		//Let's hide the cover point until it needs to be shown
		GetComponent<Renderer>().enabled = false;
		tr = transform;
	}

	public void FillCover(GameObject unit) 
	{
		isFilled = true;
		filledBy = unit;
	}

	public void UnfillCover()
	{
		if (isFilled) isFilled = false;

		if (filledBy != null) {
			filledBy = null;
		}
	}

	public Vector3 Position {
		get { return tr.position; }
	}
}
