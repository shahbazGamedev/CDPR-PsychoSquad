using UnityEngine;
using System.Collections;

/// <summary>
/// Loading utility that does scene fading.
/// </summary>
[RequireComponent(typeof(Fading))]
public class LoadingUtil : Singleton<LoadingUtil> {
	static protected Fading fade;

	void Start() {
		SetFade ();
	}

	static protected void SetFade () {
		fade = Instance.GetComponent<Fading>();

		if (fade.fadeOutTexture != null) return;

		Texture2D tex = new Texture2D(16,16);
		Color[] cols = new Color[tex.width*tex.height];
		for (int i = 0; i < cols.Length; i++) {
			cols[i] = Color.black;	//new Color(Random.value, Random.value, Random.value);
		}
		tex.SetPixels(cols);
		tex.Apply(false);

		fade.fadeOutTexture = tex;
	}

	static public void BeginLoad() {
		if (fade == null) SetFade();

		fade.BeginFade(1);
	}

	static public void EndLoad() {
		if (fade == null) SetFade();

		fade.BeginFade(-1);
	}
}
