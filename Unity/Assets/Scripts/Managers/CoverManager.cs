using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Cover manager tool that helps retrieve local cover on request.
/// </summary>
public class CoverManager : Singleton<CoverManager> {
	[HideInInspector]
	public CoverPoint[] cover;

	void OnLevelWasLoaded(int i) {
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game") {
			cover = FindObjectsOfType<CoverPoint>();
		}
	}

	public static Vector3 GetClosestCoverPosition (Vector3 position) {
		CoverPoint cp = GetClosestCoverPoint(position);

		return (cp != null ? cp.Position : Vector3.zero);
	}

	/// <summary>
	/// Gets the closest cover point to this position
	/// </summary>
	/// <returns>The closest cover point.</returns>
	/// <param name="position">Position.</param>
	public static CoverPoint GetClosestCoverPoint (Vector3 position) {
		CoverPoint cp = null;

		float dist = float.MaxValue, d = 0f;
		for (int i = 0; i < Instance.cover.Length; i++) {
			if ((d = (Instance.cover[i].Position-position).sqrMagnitude) < dist) {
				dist = d;
				cp = Instance.cover[i];
			}
		}
		
		return cp;
	}

	public static Vector3 GetClosestPositionBetween (Vector3 posA, Vector3 posB) {
		CoverPoint cp = GetClosestPointBetween (posA, posB);

		return (cp != null ? cp.Position : Vector3.zero);
	}

	/// <summary>
	/// Gets the closest point between posA and posB with a preference towards
	/// finding the closest point to A.
	/// </summary>
	/// <returns>The <see cref="CoverPoint"/>.</returns>
	/// <param name="posA">Position a.</param>
	/// <param name="posB">Position b.</param>
	public static CoverPoint GetClosestPointBetween(Vector3 posA, Vector3 posB) {
		float baseDist = (posA-posB).sqrMagnitude;
		float dA = 0f, distA = float.MaxValue;
		//float dB = 0f, distB = float.MaxValue;

		CoverPoint cp = null;

		for (int i = 0; i < Instance.cover.Length; i++) {
			if ((dA = (Instance.cover[i].Position-posA).sqrMagnitude) < baseDist && dA < distA &&
			    (/*dB = */(Instance.cover[i].Position-posB).sqrMagnitude) < baseDist/* && dB < distB*/) {
				distA = dA;
				//distB = dB;
				cp = Instance.cover[i];
			}
		}

		return cp;
	}

	public static CoverPoint[] GetPointsWithin(Vector3 pos, Vector2 range) {
		List<CoverPoint> cps = new List<CoverPoint>();

		float minDist = range.x*range.x, maxDist = range.y*range.y;

		for (int i = 0; i < Instance.cover.Length; i++) {
			if ((Instance.cover[i].Position-pos).sqrMagnitude <= maxDist &&
			    (Instance.cover[i].Position-pos).sqrMagnitude >= minDist) {
				cps.Add (Instance.cover[i]);
			}
		}

		if (cps.Count > 0) {
			return cps.ToArray();
		} else {
			return null;
		}
	}

	/*public static bool CellOccupied (int ndx) {
		GameObject[] units = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject unit in units)
		{
			//if (ndx == Instance.pgc.PathGrid.GetCellIndex(unit.transform.position)) return true;
		}
		return false;
	}*/
}
