using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Wait canvas UI class (for when AI turn).
/// </summary>
public class WaitCanvas : MonoBehaviour {
	Color textColor = Color.white;
	Color outlineColor = Color.black;
	float alpha = 0f;
	float fadeTime = 1f;
	Text turnText;
	Outline outline;

	void Start() {
		LinkReferences();
	}

	public void Show(bool enable = true) {
		StopCoroutine("doPulse");
		gameObject.SetActive(enable);
		if (enable && LinkReferences()) {
			
			StartCoroutine("doPulse");
		}
	}

	IEnumerator doPulse() {
		alpha = 0f;
		bool increase = true;

		while ((alpha < 1f && increase) || (alpha > 0f && !increase)) {
			alpha = Mathf.Clamp01(Mathf.MoveTowards(alpha, (increase ? 1f : 0f), fadeTime*Time.deltaTime));
			if ((increase && alpha >= 1f) || (!increase && alpha <= 0f)) 
				increase = !increase;

			textColor.a = outlineColor.a = alpha;
			turnText.color = textColor;
			outline.effectColor = outlineColor;
			
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	bool LinkReferences() {
		if (turnText != null && outline != null) return true;

		turnText = GetComponentInChildren<Text>();
		outline = GetComponentInChildren<Outline>();

		return (turnText != null && outline != null);
	}
}
