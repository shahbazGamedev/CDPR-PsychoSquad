using UnityEngine;
using System.Collections;

/// <summary>
/// Fading script that is used by LoadingUtil.
/// </summary>
public class Fading : MonoBehaviour {

	public Texture2D fadeOutTexture;
	public float fadeSpeed = 1f;

	private int drawDepth = -1000;
	private float alpha = 1.0f;
	private float fadeDir = -1;

	void OnGUI()
	{
		if ((fadeDir > 0 && alpha >= 1f) || (fadeDir < 0 && alpha <= 0f)) {
			// do nothing
		} else {
			alpha += fadeDir * fadeSpeed * Time.deltaTime;
		}

		alpha = Mathf.Clamp01 (alpha);

		GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, alpha);
		GUI.depth = drawDepth;
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeOutTexture);
	}

	public float BeginFade (float direction) 
	{
		fadeDir = direction;
		return (fadeSpeed);
	}
}