public interface IAbility {
    IWeapon Clone();
    bool IsOnCooldown();
    int TurnsUntilReady();
    void Use();
    void NewTurn();
}
