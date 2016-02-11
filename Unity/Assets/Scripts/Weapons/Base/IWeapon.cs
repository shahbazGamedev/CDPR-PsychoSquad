public interface IWeapon {
	IWeapon Clone();
    bool HasAmmoLoaded();
    bool HasAmmoSpare();
    bool IsOnCooldown();
    int RoundsLoaded();
    int RoundsSpare();
    float GetWeaponRange();
    void Fire();
    void Reload();
    void SetFireRate(int rate);
}
