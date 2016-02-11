using UnityEngine;
using System.Collections;

/// <summary>
/// Entity class for all units.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Entity : MonoBehaviour, ILivingEntity {
	#region Character Content Vars
	[Header("Content")]
	//public string description = "Description of character.";
	//public int cost = 100;
	public Sprite portrait;
	//public Sprite hiddenPortrait;
	#endregion

	#region Stats
	[Header("Stats")]
	[SerializeField] private float maxHealth = 100f;	// maximum health stat
	[SerializeField] private float health = 100f;		// current health stat
	[SerializeField] private float moveRange = 10f;		// maximum distance this unit can move
	[SerializeField] private float moveRemaining = 10f;	// nodes this unit can travel still
	[SerializeField] private float sightRange = 20f;	// max distance this unit can see
	[SerializeField] private float hearingRange = 50f;	// max distance this unit can hear
	[SerializeField, Range(0f,1f)] private float accuracy = 1f;	// character's accuracy rating
	#endregion

	#region Specialization
	[Header("Traits")]
	[SerializeField] private Personality.PersonalityType personalityType = Personality.PersonalityType.Neutral;	// personality type of entity
	[SerializeField] private WeaponType weaponType = WeaponType.AssaultRifle;	// weapon type used by entity
	[SerializeField] private AbilityType abilityType = AbilityType.Heal;		// ability type used by entity
	#endregion

	#region References
	[Header("References")]
	public Transform eyesTransform;		// assign a transform to represent eye level
	public GameObject model;			// parent of character model
	private CapsuleCollider capsuleCollider; // used for LOS detection
	private DamageText damageText;		// displays damage stats over receiver's head
	#endregion

	#region Interface Vars
	[HideInInspector] public bool isSelected = false;	// currently active?
	[HideInInspector] public bool isPossessed = false;	// human or AI control
	[HideInInspector] public bool turnComplete = false;
	[HideInInspector] public bool isMoving = false;
	[HideInInspector] public bool isAttacking = false;
	[HideInInspector] public bool isStunned = false;
	[HideInInspector] public bool takingTurn = false;
	#endregion

	#region Entity Tools
	private GoalThink brain;		// unit brain/order execution
	private Senses senses;			// unit's sensorium (sight/sound/etc)
	private TargetSystem targetSystem;	// unit's targeting class
	private BaseGun weapon;			// unit's weapon reference
	private AbilityBase ability;	// unit's special ability reference
	private Personality personality;// unit's established personality

	private TeamManager team;		// team unit belongs to
	private NavMeshAgent agent;		// pathfinding agent
	private AutoCover autoCover;	// dynamic cover class
	private NavMeshObstacle dynamicObstacle;	// pathfinding obstacle ref
	#endregion

	#region Cached Vars
	[HideInInspector] public Transform tr;	// cached transform
	#endregion

	#region Testing Vars
	private float visibilityUpdateRate = 2f;	// rate to update unit's visibility to enemies
	private float lastVisibilityUpdate = 0f;
	#endregion

	#region MonoBehaviour Methods
	// Use this for initialization
	void Start () {
		tr = transform;
		agent = GetComponent<NavMeshAgent>();
		capsuleCollider = GetComponentInChildren<CapsuleCollider>();
		damageText = GetComponentInChildren<DamageText>();
		health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
		if (takingTurn && isAlive) {
			StartCoroutine(ProcessBrain());
		}

		if (team.teamType != TeamType.Human && isAlive) {
			VisibilityCheck();
		}
	}

	void OnEnable() {
		GameManager.OnEntityEvent += EntityEventHandler;
	}

	void OnDisable() {
		brain.OnEvent -= BrainEventHandler;
		GameManager.OnEntityEvent -= EntityEventHandler;
	}
	#endregion

	#region Local/Helper Methods
	/// <summary>
	/// Processes the brain, executing existing goals or selecting new ones.
	/// </summary>
	/// <returns>The brain.</returns>
	IEnumerator ProcessBrain() {
		brain.Process();
		yield return null;
	}

	/// <summary>
	/// Checks for visibility to enemy players (visual reference).
	/// </summary>
	void VisibilityCheck() {
		if ((Time.time-lastVisibilityUpdate) < visibilityUpdateRate) return;

		if (GameManager.IsVisibleByPlayer(this)) {
			if (!model.activeSelf) {
				model.SetActive(true);
				capsuleCollider.isTrigger = false;
			}
		} else {
			if (model.activeSelf) {
				model.SetActive(false);
				capsuleCollider.isTrigger = true;
			}
		}
		lastVisibilityUpdate = Time.time;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Enables/disables the agent/dynamic obstacle.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void EnableAgent(bool enable = true) {
		StopCoroutine("DelayedEnable");
		StartCoroutine("DelayedEnable",enable);
	}

	private IEnumerator DelayedEnable(bool enable) {
		yield return new WaitForFixedUpdate();
		if (enable) {
			dynamicObstacle.carving = false;
			dynamicObstacle.enabled = false;
			yield return new WaitForFixedUpdate();
			while(dynamicObstacle.enabled) {
				dynamicObstacle.enabled = false;
				yield return new WaitForFixedUpdate();
			}
			agent.enabled = true;
		} else {
			agent.enabled = false;
			yield return new WaitForFixedUpdate();
			while(agent.enabled) {
				agent.enabled = false;
				yield return new WaitForFixedUpdate();
			}
			dynamicObstacle.enabled = true;
			dynamicObstacle.carving = true;
			dynamicObstacle.carveOnlyStationary = true;
		}

		yield return null;
	}

	/// <summary>
	/// Determine if a path is available to target position
	/// </summary>
	/// <returns><c>true</c>, if path was evaluated successfully/completely, <c>false</c> otherwise.</returns>
	/// <param name="pos">Position.</param>
	public bool EvaluatePath(Vector3 pos) {
		if (!agent.enabled) { 
			EnableAgent();
			Debug.LogWarning("Attempted to evaluate a path with agent not yet activated.");
		}

		NavMeshPath path = new NavMeshPath();
		agent.CalculatePath(pos, path);

		bool valid = (path.status == NavMeshPathStatus.PathComplete);

		path.ClearCorners();
		path = null;

		return valid;
	}

	/// <summary>
	/// Restore character to original settings.
	/// </summary>
	public void Reset() {
		health = maxHealth;
		model.SetActive(true);
		capsuleCollider.enabled = true;
		capsuleCollider.isTrigger = false;
		EnableAgent(false);
		TurnReset();
	}

	/// <summary>
	/// Generates classes/references this unit will use.
	/// </summary>
	/// <param name="t">T.</param>
	public void Init(TeamManager t) {
		team = t;

		// assigning your weapon
		if (weaponType == WeaponType.AssaultRifle) {
			weapon = new WeaponAssaultRifle();
		} else if (weaponType == WeaponType.SMG) {
			weapon = new WeaponSMG();
		} else if (weaponType == WeaponType.Pistols) {
			weapon = new WeaponPistols();
		} else if (weaponType == WeaponType.Sniper) {
			weapon = new WeaponSniperRifle();
		} else if (weaponType == WeaponType.LMG) {
			weapon = new WeaponLMG();
		} else {
			Debug.LogError("No weapon assigned to unit, set in inspector.");
			weapon = new WeaponAssaultRifle();
		}

		// assign special ability
		if (abilityType == AbilityType.Ammo) {
			ability = new AmmoAbility(this);
		} else if (abilityType == AbilityType.Heal) {
			ability = new HealAbility(this);
		} else if (abilityType == AbilityType.Stealth) {
			ability = new StealthAbility(this);
		} else if (abilityType == AbilityType.Scan) {
			ability = new ScanAbility(this);
		} else if (abilityType == AbilityType.Armor) {
			ability = new ArmorAbility(this);
		} else if (abilityType == AbilityType.Accuracy) {
			ability = new AccuracyAbility(this);
		} else {
			ability = new AmmoAbility(this);
		}

		senses = new Senses(this, sightRange, hearingRange);
		targetSystem = new TargetSystem(this);
		personality = new Personality(personalityType);

		brain = new GoalThink(this);
		brain.OnEvent += BrainEventHandler;

		agent = GetComponent<NavMeshAgent>();
		autoCover = GetComponent<AutoCover>();
		dynamicObstacle = GetComponent<NavMeshObstacle>();

		moveRemaining = moveRange;
	}

	/// <summary>
	/// Sends the entity event to the GameManager for dispersal.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="eType">E type.</param>
	public void SendEvent(GameObject from, EntityEventType eType) {
		EntityEvent e = new EntityEvent(team,
			gameObject,
			from,
			tr.position,
			Time.time,
			eType);

		GameManager.SendEntityEvent(e);
	}

	/// <summary>
	/// This is called by the TeamManager controlling this unit. Begins the turn by starting the
	/// unit's brain.
	/// </summary>
	public void BeginTurn() {
		brain.Activate();

		takingTurn = true;
	}

	/// <summary>
	/// This is called by the brain process of this unit when it is finished executing its goals, or
	/// it can also be called by a human team via the UI (skip unit/endturn).
	/// Completes the turn by sending an event the TeamManager should be listening for to move to
	/// the next available unit or end the team's turn.
	/// </summary>
	public void CompleteTurn() {
		takingTurn = false;
		turnComplete = true;
		brain.HasMoved = false;
		brain.HasAttacked = false;

		SendEvent(null, EntityEventType.TurnComplete);
	}

	/// <summary>
	/// This occurs at the end of every turn.
	/// </summary>
	public void TurnReset () {
		// reset the distance you can move for when your next turn starts
		moveRemaining = moveRange;
		isSelected = isPossessed = turnComplete = takingTurn = isMoving = isAttacking = false;
	}

	public void SetSelected() {
		isSelected = true;
		if (team.teamType == TeamType.Human)
			isPossessed = true;

		SendEvent(null, EntityEventType.Selected);
	}

	public void Attack() {
		// typically perform some sort of logic here...
		SendEvent(null, EntityEventType.Attack);

		// audio cues
		bool near = Toolbox.DistanceCheck(Toolbox.Get2D(Position), Toolbox.Get2D(FollowMouse.Position), 25f);
		if (weaponType == WeaponType.AssaultRifle) {
			AudioManager.PlayAR(near);
		} else if (weaponType == WeaponType.SMG) {
			AudioManager.PlaySMG(near);
		} else if (weaponType == WeaponType.Pistols) {
			AudioManager.PlayPistol(near);
		} else if (weaponType == WeaponType.Sniper) {
			AudioManager.PlaySniper(near);
		} else if (weaponType == WeaponType.LMG) {
			AudioManager.PlayLMG(near);
		}

		GameObject tgt = targetSystem.target;
		Entity u = tgt.GetComponent<Entity>();

		// distance check against accuracy (higher = better chance, this value is added)
		// this would be better by plotting on an animation curve
		float targetDist = 1f-Mathf.Clamp01((Position-u.Position).magnitude/weapon.GetWeaponRange());

		// putting our calc for cover here (higher = better cover/worse chance, this value is subtracted)
		float targetCover = u.Cover.GetCoverageFromDir((Position-u.Position));

		// character accuracy and weapon accuracy (lower is better, this value is subtracted)
		float acc = (1f-accuracy)+(1f-weapon.GetAccuracy());
		// starting point of 1f, sub random value 0-1, sub target's cover value, sub accuracy, add dist
		float r = 0f;
		// check if we should have an accuracy bonus from abilities
		AccuracyComponent comp = GetComponent<AccuracyComponent>();
		if (comp != null && comp.IsEnabled) {
			r = Mathf.Clamp01((1f-acc-targetCover)+(comp.AccuracyRating+targetDist));
		} else {
			r = Mathf.Clamp01(1f-Random.value-targetCover-acc+targetDist);
		}

		//Debug.Log ("Accuracy Total: " + r.ToString() + "/1f");
		int rndsHit = Mathf.RoundToInt(r*(float)weapon.GetWeaponFireRate());
		//Debug.Log ("Rounds hit: " + rndsHit.ToString() + "/" + weapon.GetWeaponFireRate().ToString());
		float dmg = (float)rndsHit*weapon.GetWeaponStrength();
		// random chance for critical hit 
		bool crit = (Random.value > 0.9f);
		// check for sneak attack (from behind)
		bool sneak = Vector3.Dot (u.tr.forward, (Position-u.Position).normalized) < -0.1f;
		if (!sneak) {
			StealthComponent stealthComp = GetComponent<StealthComponent>();
			if (stealthComp != null && stealthComp.IsEnabled) {
				sneak = true;
			}
		}

		if (dmg > 0f) {
			if (crit) {
				dmg *= 1.5f;
				//Debug.Log ("Critical hit!");
			}
			if (sneak) {
				// attacking from behind
				dmg *= 1.75f;
				//Debug.Log ("Sneak attack!");
			}
		}
		targetSystem.target.GetComponent<Entity>().TakeDamage(dmg, gameObject, crit, sneak);

		Debug.DrawLine (Position, TargetSystem.TargetPosition, Color.red, 1f);
	}

	/// <summary>
	/// Play reload animation.
	/// </summary>
	public void Reload() {}

	/// <summary>
	/// Uses the special ability.
	/// </summary>
	public void UseSpecial() {
		if (ability != null) {
			bool ok = true;

			if (ability.inUse || ability.IsOnCooldown()) {
				ok = false;
			} else if (ability.usesMove && Brain.HasMoved) {
				ok = false;
			} else if (ability.usesAttack && Brain.HasAttacked) {
				ok = false;
			}

			if (ok) {
				ability.Use();
				if (model.activeSelf) damageText.ShowSpecial(ability.name);
			}
		}
	}

	/// <summary>
	/// Removes the monobehaviour component from this object.
	/// </summary>
	/// <param name="script">Script.</param>
	public void RemoveComponent(MonoBehaviour script) {
		Destroy(script);
	}
	#endregion

	#region ILivingEntity Methods
	/// <summary>
	/// Deals damage to entity.
	/// </summary>
	/// <returns><c>true</c>, if damage results in death, <c>false</c> otherwise.</returns>
	/// <param name="dmg">Damage.</param>
	/// <param name="dealer">Dealer.</param>
	public bool TakeDamage(float dmg, GameObject dealer, bool criticalHit = false, bool sneakAttack = false) {
		if (!isAlive) return false;

		if (dmg > 0f) {
			ArmorComponent armor = GetComponent<ArmorComponent>();
			if (armor != null && armor.IsEnabled) {
				dmg *= armor.ArmorRating;
			}
		}

		health = Mathf.Clamp(health-dmg, 0f, maxHealth);
		SendEvent(dealer, EntityEventType.Damaged);
		if (damageText != null) damageText.ShowDamage(dmg, criticalHit, sneakAttack);

		if (health == 0f) {
			Die(dealer);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Kills this entity.
	/// </summary>
	/// <param name="dealer">Dealer.</param>
	public void Die(GameObject dealer) {
		SendEvent(dealer, EntityEventType.Death);

		dynamicObstacle.enabled = false;
		model.SetActive(false);
		capsuleCollider.enabled = false;
	}
	#endregion

	#region Event Listeners
	void BrainEventHandler(BrainEvent e) {
		if (e == BrainEvent.Finished) {
			if (team.teamType == TeamType.AI) {
				brain.Terminate();
				CompleteTurn();
			}
		}
	}

	void EntityEventHandler(EntityEvent e) {
		if (e.eventObject == gameObject && e.eventType == EntityEventType.MoveComplete) {
			EnableAgent(false);
		}
	}
	#endregion

	#region Accessors
	public bool isAlive {
		get { return (health > 0f); }
	}

	public float Health {
		get { return health; }
	}
	public float MaxHealth {
		get { return maxHealth; }
	}

	public Vector3 Position {
		get { return tr.position; }
	}

	public float SightRange {
		get { return sightRange; }
	}

	public float HearingRange {
		get { return hearingRange; }
	}

	public float MoveRange {
		get { return moveRange; }
	}

	public float MoveRemaining {
		get { return moveRemaining; }
		set { moveRemaining = value; }
	}

	public BaseGun Weapon {
		get { return weapon; }
	}

	public AbilityBase Ability {
		get { return ability; }
	}
	public AbilityType AbilityType {
		get { return abilityType; }
	}

	public AutoCover Cover {
		get { return autoCover; }
	}

	public NavMeshAgent Agent {
		get { return agent; }
	}

	public NavMeshObstacle DynamicObstacle {
		get { return dynamicObstacle; }
	}

	public GoalThink Brain {
		get { return brain; }
	}

	public TargetSystem TargetSystem {
		get { return targetSystem; }
	}

	public Senses Senses {
		get { return senses; }
	}

	public Personality Personality {
		get {  return personality; }
	}

	public TeamManager Team {
		get { return team; }
	}
	#endregion
}

#region Entity Setup
public interface ILivingEntity {
	bool TakeDamage(float amt, GameObject from, bool criticalHit, bool sneakAttack);
	void Die(GameObject from);
}

public class Personality {
	public enum PersonalityType {
		Neutral = 0,
		Aggressive,
		Defensive
	}

	public PersonalityType personality = PersonalityType.Neutral;
	public float healthBias = 0f;
	public float hideBias = 0f;
	public float exploreBias = 0f;
	public float attackBias = 0f;
	public float reloadBias = 0f;

	public Personality() {}
	public Personality(PersonalityType pType) {

		if (pType == PersonalityType.Neutral) {
			healthBias = 1f;
			hideBias = 0.5f;
			exploreBias = 0.6f;
			attackBias = 0.5f;
			reloadBias = 0.7f;
		} else if (pType == PersonalityType.Aggressive) {
			healthBias = 0.5f;
			hideBias = 0.1f;
			exploreBias = 0.7f;
			attackBias = 0.8f;
			reloadBias = 0.5f;
		} else {
			healthBias = 1f;
			hideBias = 0.8f;
			exploreBias = 0.4f;
			attackBias = 0.4f;
			reloadBias = 0.8f;
		}
	}
}

public enum WeaponType {
	NONE = 0,
	AssaultRifle,
	Sniper,
	LMG,
	SMG,
	Pistols
}
#endregion
