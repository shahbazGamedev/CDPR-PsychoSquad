using UnityEngine;
using System.Collections;

/// <summary>
/// Single-fire, high-damage/accuracy sniper rifle weapon class.
/// </summary>
public class WeaponSniperRifle : BaseGun {
    
    public WeaponSniperRifle()
    {
		name = "Sniper Rifle";
        ammoPerMagazine = 4;
        ammoSpare = 32;
		ammoLoaded = ammoPerMagazine;
        fireRate = 1;
        range = 20f;
		damage = 50f;
		accuracy = 1f;
    }
}
