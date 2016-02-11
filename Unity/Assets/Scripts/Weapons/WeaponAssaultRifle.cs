using UnityEngine;
using System.Collections;

/// <summary>
/// Moderate-all around automatic weapon.
/// </summary>
public class WeaponAssaultRifle : BaseGun {

    public WeaponAssaultRifle()
    {
		name = "Assault Rifle";
        ammoPerMagazine = 30;
        ammoSpare = 600;
        ammoLoaded = ammoPerMagazine;
        fireRate = 6;
        range = 14f;
		damage = 7f;
		accuracy = 0.8f;
    }
}
