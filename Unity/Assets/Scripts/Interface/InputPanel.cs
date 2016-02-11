using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Input panel class for setting keys in Options.
/// </summary>
public class InputPanel : MonoBehaviour {
	public Text inputName;
	public Text primaryName;
	public Text secondaryName;
	private int index = -1;

	public void Set(string n, string p, string s, int i) {
		inputName.text = n;
		primaryName.text = p;
		secondaryName.text = s;
		index = i;
	}

	void ChangeFinished() {
		OptionsManager.ChangeKey(index);

		cInput.OnKeyChanged -= ChangeFinished;
	}

	public void ChangeKey(bool primary = true) {
		cInput.OnKeyChanged += ChangeFinished;

		cInput.ChangeKey(index, (primary ? 1 : 2));
	}

	public int Index {
		get { return index; }
	}
}
