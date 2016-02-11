using UnityEngine;
using System.Collections;

/// <summary>
/// AOE explosive grenade class.
/// </summary>
public class WeaponGrenade : BaseGun {
    
    public WeaponGrenade()
    {
		name = "Grenade";
        ammoPerMagazine = 9999;
        ammoSpare = 9999;
        ammoLoaded = 9999;
        fireRate = 1;
        range = 6;
		damage = 75f;
		accuracy = 1f;
    }
}
