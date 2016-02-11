using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class that shows damage or status text effects.
/// </summary>
public class DamageText : MonoBehaviour {
	public Text damagePopup;
	float popupValue = 0f;
	float popupTime = 0f;
	float basePopupTime = 1f;
	bool showingPopup = false;
	Color c;

	void Start() {
		damagePopup.gameObject.SetActive(false);
	}

	/// <summary>
	/// Shows the damage taken.
	/// </summary>
	/// <param name="amt">Amt.</param>
	/// <param name="criticalHit">If set to <c>true</c> critical hit.</param>
	/// <param name="sneakAttack">If set to <c>true</c> sneak attack.</param>
	public void ShowDamage(float amt, bool criticalHit, bool sneakAttack) {
		if (damagePopup != null) {
			transform.rotation = Quaternion.LookRotation((transform.position-Camera.main.transform.position).normalized);

			if (amt < 0f) c = Color.green;
			else if (criticalHit) c = Color.yellow;
			else if (sneakAttack) c = Color.red;
			else c = Color.white;

			if (!showingPopup || Mathf.Sign(popupValue) != Mathf.Sign(amt)) {
				StopCoroutine("doPopup");
				popupValue = amt;
				StartCoroutine("doPopup", (amt != 0f ? (amt.ToString() + (criticalHit ? " Crit!" : "") + (sneakAttack ? " Sneak!" : "")) : "Miss!"));
			} else {
				popupValue += amt;
				damagePopup.text = (popupValue.ToString() + (criticalHit ? " Crit!" : "") + (sneakAttack ? " Sneak!" : ""));
				popupTime += basePopupTime;
			}
		}
	}

	/// <summary>
	/// Shows the special ability name.
	/// </summary>
	/// <param name="txt">Text.</param>
	public void ShowSpecial(string txt) {
		if (damagePopup != null) {
			transform.rotation = Quaternion.LookRotation((transform.position-Camera.main.transform.position).normalized);

			c = Color.cyan;
			popupValue = 0f;
			if (!showingPopup) {
				StopCoroutine("doPopup");
				StartCoroutine("doPopup", txt);
			} else {
				damagePopup.text = txt;
				popupTime += basePopupTime;
			}
		}
	}

	IEnumerator doPopup(string amt) {
		showingPopup = true;
		popupTime = basePopupTime;
		c.a = 0f;
		damagePopup.color = c;
		damagePopup.gameObject.SetActive(true);
		damagePopup.text = amt;
		while (c.a < 1f) {
			c.a = Mathf.Clamp01(c.a+Time.deltaTime);
			damagePopup.color = c;
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(popupTime);
		showingPopup = false;
		while (c.a > 0f) {
			c.a = Mathf.Clamp01(c.a-Time.deltaTime);
			damagePopup.color = c;
			yield return new WaitForEndOfFrame();
		}
		damagePopup.gameObject.SetActive(false);
		popupValue = 0;
		popupTime = basePopupTime;
		yield return null;
	}
}
