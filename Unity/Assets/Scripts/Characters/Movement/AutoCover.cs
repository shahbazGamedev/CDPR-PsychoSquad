using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grid cover status for cardinal directions.
/// </summary>
public class GridCoverStatus {
	public Cover North;
	public Cover Northeast;
	public Cover East;
	public Cover Southeast;
	public Cover South;
	public Cover Southwest;
	public Cover West;
	public Cover Northwest;

	private Cover[] coverage;

	public GridCoverStatus() {
		North = new Cover(Vector3.forward);
		Northeast = new Cover((Vector3.forward + Vector3.right));
		East = new Cover(Vector3.right);
		Southeast = new Cover((Vector3.back + Vector3.right));
		South = new Cover(Vector3.back);
		Southwest = new Cover((Vector3.back + Vector3.left));
		West = new Cover(Vector3.left);
		Northwest = new Cover((Vector3.forward + Vector3.left));

		coverage = new Cover[8]{
			North, Northeast, East, Southeast, South, Southwest, West, Northwest
		};
	}

	public Cover[] Coverage {
		get { return coverage; }
	}
}

/// <summary>
/// Class that defines a single cover position.
/// </summary>
public class Cover {
	public Vector3 dir;
	public CoverStatus cover;
	public CoverObject coverObject;
	public float coverRating;

	public Cover() {
		dir = Vector3.zero;
		cover = CoverStatus.None;
		coverObject = null;
		coverRating = 0f;
	}
	public Cover(Vector3 d) {
		dir = d;
		cover = CoverStatus.None;
		coverObject = null;
		coverRating = 0f;
	}
}

/// <summary>
/// Class that is designed to easily discover/update cover status in the game world.
/// </summary>
public class AutoCover : MonoBehaviour {

	public bool debug = false;
	public bool drawGizmos = true;
	public float maxCoverDistance = 1f;

	public GridCoverStatus status;

	public Vector3 halfCoverLevel = new Vector3(0, 0.25f, 0);
	public Vector3 fullCoverLevel = new Vector3(0, 0.75f, 0);

	[HideInInspector]
	public Transform tr;
	private int raycastLayerMask;
	private float height = 2f;

	private float lastUpdateTime = 0f;
	private float minUpdateInterval = 0.3f;

	#region MonoBehaviour Methods
	void Awake() {
		status = new GridCoverStatus();
		tr = transform;
	}

	void Start () {
		raycastLayerMask = ~LayerMask.GetMask("Player");

		if (GetComponent<Collider>() != null) {
			height = (GetComponent<Collider>() as CapsuleCollider).height;
		}

		halfCoverLevel.y = height*halfCoverLevel.y;
		fullCoverLevel.y = height*fullCoverLevel.y;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Returns the results of an omnidirectional raycast check as a percentage of cover.
	/// </summary>
	/// <param name="location">Location</param>
	/// <returns>float cover</returns>
	public float CalculateTotalCover()
	{
		UpdateCover ();
		float coverHits = 0f;

		for (int i = 0; i < status.Coverage.Length; i++) {
			coverHits += status.Coverage[i].coverRating;
		}
		
		float cover = (coverHits / 16f) * 100f;
		if (debug) Debug.Log ("Cover Percentage: " + cover.ToString() + "%");
		return cover;
	}



	/// <summary>
	/// Updates unit's current coverage.
	/// </summary>
	public void UpdateCover()
	{
		for (int i = 0; i < status.Coverage.Length; i++) {
			CheckForHits (status.Coverage[i]);
		}
		lastUpdateTime = Time.time;
	}

	/// <summary>
	/// Gets the coverage from a direction.
	/// </summary>
	/// <returns>The coverage percentage in direction scaled 0-1f.</returns>
	/// <param name="dir">Direction from this object.</param>
	public float GetCoverageFromDir(Vector3 dir) {
		if ((Time.time-lastUpdateTime) >= minUpdateInterval)
			UpdateCover();

		float pcn = 0f;
		float a = 0f;

		for (int i = 0; i < status.Coverage.Length; i++) {
			if (Vector3.Dot (status.Coverage[i].dir, dir.normalized) > 0.4f) {
				a += 1f;
				pcn += status.Coverage[i].coverRating;
				if (status.Coverage[i].coverRating > 1f)
					Debug.DrawLine (tr.position+fullCoverLevel, tr.position+status.Coverage[i].dir+fullCoverLevel, Color.green, 0.3f);
				else if (status.Coverage[i].coverRating > 0f)
					Debug.DrawLine (tr.position+halfCoverLevel, tr.position+status.Coverage[i].dir+halfCoverLevel, Color.cyan, 0.3f);
			}
		}

		pcn = (pcn*0.5f)/a;
		if (debug) Debug.Log ("CoverFromDirection : " + (pcn*100f).ToString() + "% from " + a.ToString() + " directions.");

		return pcn;
	}
	#endregion

	#region Helper Methods
	/// <summary>
	/// Sends raycasts in multiple directions/heights to update cover statuses.
	/// </summary>
	/// <returns><c>true</c>, if cover found, <c>false</c> otherwise.</returns>
	/// <param name="cover">Cover.</param>
	bool CheckForHits(Cover cover)
	{
		Vector3 origin = tr.position;
		Vector3 direction = cover.dir;

		GameObject hitTarget = null;
		RaycastHit hit;
		// chest-height raycast
		if (Physics.Linecast(origin+fullCoverLevel, origin+fullCoverLevel+(direction*maxCoverDistance), out hit, raycastLayerMask)) {
			hitTarget = hit.transform.gameObject;
			if (drawGizmos) Debug.DrawRay(origin+fullCoverLevel, direction * maxCoverDistance, Color.green, 0.3f);
			
			if (hitTarget != null) {
				CoverObject co = hitTarget.GetComponent<CoverObject>();
				if (co != null) {
					cover.coverObject = co;
					cover.cover = CoverStatus.Full;
					cover.coverRating = 2f;
					
					return true;
				}
			}
		}

		// knee-height raycast
		if (Physics.Linecast(origin+halfCoverLevel, origin+halfCoverLevel+(direction*maxCoverDistance), out hit, raycastLayerMask)) {
			hitTarget = hit.transform.gameObject;
			if (drawGizmos) Debug.DrawRay(origin+halfCoverLevel, direction * maxCoverDistance, Color.cyan, 0.3f);
			
			if (hitTarget != null) {
				CoverObject co = hitTarget.GetComponent<CoverObject>();
				if (co != null) {
					cover.coverObject = co;
					cover.cover = CoverStatus.Half;
					cover.coverRating = 1f;
					
					return true;
				}
			}
		}

		cover.coverObject = null;
		cover.cover = CoverStatus.None;
		cover.coverRating = 0f;
		if (drawGizmos) Debug.DrawRay(origin+(Vector3.up*0.5f), direction * maxCoverDistance, Color.yellow, 0.3f);

		return false;
	}
	#endregion
}
