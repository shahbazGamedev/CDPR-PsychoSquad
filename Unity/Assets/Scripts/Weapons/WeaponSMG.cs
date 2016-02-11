using UnityEngine;
using System.Collections;

/// <summary>
/// Fast-firing, low accuracy SMG class.
/// </summary>
public class WeaponSMG : BaseGun {
	public WeaponSMG()
	{
		name = "Submachine Gun";
		ammoPerMagazine = 30;
		ammoSpare = 120;
		ammoLoaded = ammoPerMagazine;
		fireRate = 8;
		range = 10f;
		damage = 7f;
		accuracy = 0.7f;
	}
}
