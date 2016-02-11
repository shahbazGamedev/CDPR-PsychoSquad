using UnityEngine;
using System.Collections;

/// <summary>
/// Short-range, high-damage, moderate accuracy pistol class.
/// </summary>
public class WeaponPistols : BaseGun {
    
    public WeaponPistols()
    {
		name = "Dual Pistols";
        ammoPerMagazine = 16;
        ammoSpare = 40;
		ammoLoaded = ammoPerMagazine;
        fireRate = 4;
        range = 9f;
		damage = 10f;
		accuracy = 0.8f;
    }
}
