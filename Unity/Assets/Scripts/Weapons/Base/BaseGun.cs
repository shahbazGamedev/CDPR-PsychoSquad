using UnityEngine;

/// <summary>
/// Base weapon class associated with weapon interface.
/// </summary>
public class BaseGun : IWeapon {
	public delegate void WeaponFired();
    public WeaponFired weaponFireSuccess;
    public WeaponFired weaponFireFail;

    public delegate void WeaponReloaded();
    public WeaponReloaded weaponReloadSuccess;
    public WeaponReloaded weaponReloadFail;

	public string name = "";

	#region Weapon Stats
    protected int ammoPerMagazine = 8;
    protected int ammoSpare = 32;
    protected int ammoLoaded = 8;
    protected int fireRate = 1;
    protected float range = 5;
    protected float damage = 10f;
	protected float accuracy = 0.8f;
	#endregion

    private bool onCooldown = false;

	public IWeapon Clone()
    {
        return (IWeapon)this.MemberwiseClone();
    }

	public int GetWeaponFireRate() {
		return fireRate;
	}

    public float GetWeaponStrength()
    {
        return damage;
    }

	public float GetAccuracy() {
		return accuracy;
	}

    public bool HasAmmoLoaded()
    {
        return ammoLoaded > 0;
    }

    public bool HasAmmoSpare()
    {
        return ammoSpare > 0;
    }

    public bool IsOnCooldown()
    {
        return onCooldown;
    }

    public int RoundsLoaded()
    {
        return ammoLoaded;
    }
    
    public int RoundsSpare()
    {
        return ammoSpare;
    }

	public Vector2 GetIdealRange() {
		Vector2 r = new Vector2(range*0.5f, range*0.9f);

		return r;
	}

    public float GetWeaponRange()
    {
        return range;
    }

	public float GetWeaponRangeSqrd() {
		return (range*range);
	}

	public int GetMagazineSize() {
		return ammoPerMagazine;
	}

    public void Fire()
    {
        //Do we have enough ammo to fire?
        if (ammoLoaded >= fireRate)
        {
            ammoLoaded -= fireRate;

            //Fire succeeded
            if (weaponFireSuccess != null)
                weaponFireSuccess();

            return;
        }

        //Fire failed
        if (weaponFireFail != null)
            weaponFireFail();
    }

    public void Reload()
    {
        //Do we have any ammo to load?
        if (HasAmmoSpare())
        {
            //How much ammo do we need to load to make a full magazine?
            int ammoNeeded = ammoPerMagazine - ammoLoaded;

            //Do we have this much ammo spare?
            if (ammoSpare >= ammoNeeded)
            {
                //Take the ammo from our spare and load the magazine
                ammoLoaded += ammoNeeded;
                ammoSpare -= ammoNeeded;
            }
            else
            {
                //Take whatever ammo is left and load it in
                ammoLoaded += ammoSpare;
                ammoSpare = 0;
            }

            //Reload succeeded
            if (weaponReloadSuccess != null)
                weaponReloadSuccess();
        }

        //Reload failed
        if (weaponReloadFail != null)
            weaponReloadFail();
    }

    public void SetFireRate(int rate)
    {
        fireRate = rate;
    }
}
