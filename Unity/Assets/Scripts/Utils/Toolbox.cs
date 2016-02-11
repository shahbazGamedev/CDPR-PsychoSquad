using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Toolbox class for simple reference to global methods.
/// </summary>
public class Toolbox {
	/// <summary>
	/// Reference to an "invalid" Vector3 that will not be used in-game.
	/// </summary>
	private static Vector3 invalidV3 = new Vector3(-999,-999,-999);

	/// <summary>
	/// Gets a Vector2 from Vector3
	/// </summary>
	/// <returns>The d.</returns>
	/// <param name="v3">V3.</param>
	public static Vector2 Get2D(Vector3 v3) {
		return new Vector2(v3.x, v3.z);
	}

	/// <summary>
	/// Wraps the index of any List.
	/// </summary>
	/// <returns>The list index.</returns>
	/// <param name="list">List.</param>
	/// <param name="index">Index.</param>
	/// <example>If an incremented/decremented index variable exceeds the list's length or
	/// is less than zero, it will wrap it around from the other direction until a valid index
	/// is reached. Common usage example would be for waypoint procession.</example>
	public static int WrapListIndex<T> (List<T> list, int index) {
		if (list == null || list.Count < 1) {
			Debug.LogWarning ("Cannot wrap a list with no elements.");
			return -1;
		}

		int ndx = index;
		bool wrapped = true;

		if (index >= list.Count) {
			ndx = (index-list.Count);
		} else if (index < 0) {
			ndx = (list.Count+index);
		} else {
			wrapped = false;
		}

		if (wrapped)
			return WrapListIndex(list, ndx);

		return ndx;
	}

	/// <summary>
	/// Check if the distance between two vectors is less than maximum specified distance.
	/// </summary>
	/// <param name="v1">First vector.</param>
	/// <param name="v2">Second vector.</param>
	/// <param name="maxDistance">Maximum distance between vectors.</param>
	/// <returns>True if distance is less than maximum distance; false otherwise.</returns>
	public static bool DistanceCheck(Vector3 v1, Vector3 v2, float maxDistance)
	{
		return ((v1-v2).magnitude <= maxDistance);
	}

	public static Vector3 SnapToGrid(Vector3 v3) {
		v3.x = Mathf.RoundToInt(v3.x);
		v3.y = Mathf.RoundToInt(v3.y);
		v3.z = Mathf.RoundToInt(v3.z);

		return v3;
	}

	public static Vector3 InvalidV3 {
		get { return invalidV3; }
	}
}
