using UnityEngine;
using System.Collections;

/// <summary>
/// Low-accuracy, high ROF LMG class.
/// </summary>
public class WeaponLMG : BaseGun {
    
    public WeaponLMG()
    {
		name = "LMG";
        ammoPerMagazine = 50;
        ammoSpare = 400;
		ammoLoaded = ammoPerMagazine;
        fireRate = 10;
        range = 10f;
		damage = 7f;
		accuracy = 0.6f;
    }
}
