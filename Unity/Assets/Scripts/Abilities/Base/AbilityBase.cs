using UnityEngine;
using System.Collections;

public enum AbilityType {
	NONE = 0,
	Heal = 1,
	Ammo = 2,
	Stealth = 3,
	Scan = 4,
	Armor = 5,
	Accuracy = 6
}

/// <summary>
/// Base class for special abilities owned by a unit.
/// </summary>
public class AbilityBase : IAbility {
	private bool onCooldown = false;	// is ability on cooldown after use
	private bool isActive = false;		// is ability current in use

	protected int duration = 1;			// how many turns the ability is effective for
    protected int cooldownDelay = 5;	// how many turns the ability is inaccessible for after usage
	protected float range = 10f;		// if range-based, how far the ability reaches in game units

    private int usedTurnNumber;			// turn number this ability was used on
    private int refreshTurnNumber;		// turn number this ability will be available on

	protected Entity owner = null;		// owner of this ability

	public bool usesMove = false;		// does using this ability sacrifice turn's move phase
	public bool usesAttack = true;		// does using this ability sacrifice turn's attack phase

	public string name = "TestSpecial";	// name of ability
	public string description = "This is where you should describe the special ability.";

	~AbilityBase() {
		GameManager.OnGameEvent -= HandleGameEvent;
	}

    public IWeapon Clone()
    {
        return (IWeapon)this.MemberwiseClone();
    }

	#region Interface Methods
	/// <summary>
	/// Determines whether this ability instance is on cooldown.
	/// </summary>
	/// <returns><c>true</c> if this instance is on cooldown; otherwise, <c>false</c>.</returns>
    public bool IsOnCooldown()
    {
        return onCooldown;
    }

	/// <summary>
	/// Turns remaining until the ability is ready again.
	/// </summary>
	/// <returns>Turns until ready.</returns>
    public int TurnsUntilReady()
    {
        return currentTurnNumber - refreshTurnNumber;
    }

	/// <summary>
	/// Use this ability instance.
	/// </summary>
    public virtual void Use()
    {
        //If we're off cooldown
        if (!onCooldown)
        {
            ActivateAbility();

			if (usesMove) {
				owner.Brain.HasMoved = true;
			}
			if (usesAttack) {
				owner.Brain.HasAttacked = true;
			}
        } else {
            //abilityUseFail();
		}
    }

	/// <summary>
	/// Event receiver for when a new turn has been initiated.
	/// </summary>
	public void NewTurn()
	{
		if (currentTurnNumber >= usedTurnNumber + cooldownDelay) 
			onCooldown = false;

		if (currentTurnNumber >= usedTurnNumber + duration) {
			isActive = false;
			DeactivateAbility();
		}
	}
	#endregion

	#region Base Methods
	/// <summary>
	/// Determines whether this instance is available.
	/// </summary>
	/// <returns><c>true</c> if this instance is available; otherwise, <c>false</c>.</returns>
	private bool IsAvailable() {
		if (usesMove && owner.Brain.HasMoved) return false;
		if (usesAttack && owner.Brain.HasAttacked) return false;

		return (!onCooldown && !isActive);
	}

	/// <summary>
	/// Activates the ability.
	/// </summary>
    protected virtual void ActivateAbility()
    {
        refreshTurnNumber = currentTurnNumber + cooldownDelay;
        
        //Set the counter to the turn we used at
        usedTurnNumber = currentTurnNumber;

        onCooldown = true;
		isActive = true;

		GameManager.OnGameEvent += HandleGameEvent;
		GameManager.Log(owner.name + " has used " + this.name);
    }

	/// <summary>
	/// Deactivates the ability.
	/// </summary>
	protected virtual void DeactivateAbility() {}
	#endregion

	#region Event Listeners
	/// <summary>
	/// Handles the game event.
	/// </summary>
	/// <param name="e">Event.</param>
	protected void HandleGameEvent (GameEvent e) {
		if (e.eventTeam == owner.Team && e.eventType == GameEventType.TurnStart) {
			NewTurn();
		}
	}
	#endregion

	#region Accessors
	protected int currentTurnNumber {
		get { return owner.Team.TurnCount; }
	}

	public int Duration {
		get { return duration; }
	}

	public float Range { 
		get { return range; }
	}
	public bool inUse {
		get { return isActive; }
	}
	public bool isAvailable {
		get { return IsAvailable(); }
	}
	#endregion
}
