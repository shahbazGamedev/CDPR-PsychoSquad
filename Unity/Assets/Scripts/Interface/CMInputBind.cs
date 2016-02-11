using UnityEngine;
using System.Collections;

/// <summary>
/// Initializes cInput component with default keys.
/// </summary>
public class CMInputBind : MonoBehaviour {
	// Use this for initialization
	void Awake () {
		cInput.Init();

		// unit selection
		cInput.SetKey("Unit 1", Keys.Alpha1);
		cInput.SetKey("Unit 2", Keys.Alpha2);
		cInput.SetKey("Unit 3", Keys.Alpha3);
		cInput.SetKey("Unit 4", Keys.Alpha4);

		// action selection
		cInput.SetKey("Move", Keys.W);
		cInput.SetKey("Attack", Keys.D);
		cInput.SetKey("Special", Keys.S);
		cInput.SetKey("Skip Unit", Keys.A);
		cInput.SetKey("Reload", Keys.R);
		cInput.SetKey("End Turn", Keys.Q);

		// menu/navigation
		cInput.SetKey("Cancel", Keys.Escape, Keys.Xbox1Start);

	}
}
