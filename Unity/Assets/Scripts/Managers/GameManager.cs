using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum Difficulty {
	Easy = 0,
	Medium = 1,
	Hard = 2,
	VeryHard = 3
}

/// <summary>
/// Game manager class that handles basic game functions and cycle.
/// </summary>
public class GameManager : Singleton<GameManager> {
	#region Team Vars
	// team tracking
	private static TeamManager[] teams;
	private static int currentTeam = -1;
	// team prefabs
	public GameObject aiTeamManager;
	public GameObject humanTeamManager;
	// team setup vars
	public int numTeams = 0;		// total teams in game
	public int numAITeams = 0;		// number of teams that are AI-controlled
	// team characters
	public GameObject[] SquadRoster;
	public GameObject[] PsychoRoster;
	// entity tracking
	private static List<Entity> entities;
	// spawning
	public static SpawnPoint[] spawns;
	#endregion

	// events
	public delegate void GameEventHandler (GameEvent e);
	public static event GameEventHandler OnGameEvent;
	public delegate void EntityEventHandler(EntityEvent e);
	public static event EntityEventHandler OnEntityEvent;

	// scoring
	private float roundTimeElapsed = 0f;
	public static Difficulty difficulty = Difficulty.VeryHard;

	// game status
	private bool isPaused { get; set; }
	private bool gameRunning { get; set; }
	private static bool levelReady = false;
	private static int seed = 0;

	// UI references
	private GameCanvas gameCanvas;
	private WaitCanvas waitCanvas;

	#region MonoBehaviours
	void OnEnable() {
		OnEntityEvent += HandleEntityEvent;
		OnGameEvent += HandleGameEvent;
	}

	void OnDisable() {
		OnEntityEvent -= HandleEntityEvent;
		OnGameEvent -= HandleGameEvent;
	}

	void Start () {
		gameRunning = false;
		isPaused = true;
	}
	
	void FixedUpdate () {
		if (gameRunning) {
			roundTimeElapsed += Time.fixedDeltaTime;
		}
	}

	void Update() {
		// listen for non-game player inputs
		if (OptionsManager.SettingControls) return;

		if (cInput.GetButtonDown("Cancel")) {
			if (SceneManager.GetActiveScene().name == "Game") 
				PauseManager.Pause(!PauseManager.Paused);
			else if (SceneManager.GetActiveScene().name == "Menu") {
				MainMenu menu = FindObjectOfType<MainMenu>();
				if (menu != null) menu.ShowQuitDialogue();
			}
		}

		GameUpdate();
	}

	void OnLevelWasLoaded(int i) {
		if (SceneManager.GetActiveScene().name == "Game") {
			OnLoadGame();
		}
	}
	#endregion

	#region Event Handlers
	void HandleEntityEvent(EntityEvent e) {
		/*if (e.eventObject != null) {
			Log (e.eventObject.name + " : " + e.eventType.ToString() + (e.eventBlame != null ? (" by " + e.eventBlame.name) : "") + " -> " + e.eventPosition.ToString() + " @ " + e.eventTime.ToString(), e.eventObject);
		}*/

		if (e.eventType == EntityEventType.Death) {
			if (e.eventTeam.EntitiesAlive() < 1) {
				GameEvent ev = new GameEvent();
				ev.eventTeam = e.eventTeam;
				ev.eventType = GameEventType.TeamLose;
				SendGameEvent (ev);
				for (int i = 0; i < teams.Length; i++) {
					if (teams[i] == e.eventTeam) continue;
					ev.eventTeam = teams[i];
					ev.eventType = GameEventType.TeamWon;
					SendGameEvent (ev);
				}
				gameRunning = false;
			}
		}
	}

	void HandleGameEvent(GameEvent e) {
		//Log((e.eventTeam != null ? e.eventTeam.name : "Unknown Team") + " : " + e.eventType.ToString());

		if (e.eventType == GameEventType.TurnStart) {
			//Log(e.eventTeam.gameObject.name + "'s turn.");
			gameCanvas.endTurnButton.interactable = (e.eventTeam.teamType == TeamType.Human);
		} else if (e.eventType == GameEventType.TeamLose) {
			LevelComplete((e.eventTeam.teamType != TeamType.Human));
		}
	}

	public static void SendGameEvent(GameEvent e) {
		OnGameEvent(e);
	}

	public static void SendEntityEvent(EntityEvent e) {
		OnEntityEvent(e);
	}
	#endregion

	#region Base Public Methods
	public static void Log(string msg, Object obj = null) {
		string time = System.DateTime.Now.ToLongTimeString();
		if (obj != null) Debug.Log(time + " [GameManager] : '" + msg + "'", obj);
		else Debug.Log(time + " [GameManager] : '" + msg + "'");
	}

	public static void StartRoundTime() {
		TimeElapsed = 0f;
		Paused = false;
		Running = true;
	}

	public static void StopRoundTime() {
		Paused = true;
		Running = false;
	}

	public static void ResetRoundTime() {
		TimeElapsed = 0f;
	}

	static public string FormatTime(string sep = ":") {
		float t = Instance.roundTimeElapsed;
		int hr = Mathf.FloorToInt(t/3600f);
		t -= (float)hr*3600f;
		int min = Mathf.FloorToInt(t/60f);
		t -= (float)min*60f;
		int sec = Mathf.FloorToInt(t);
		t -= (float)sec;
		int ms = (int)(t*1000f);

		return (hr.ToString("D2") + sep + min.ToString("D2") + sep + sec.ToString("D2") + "." + ms.ToString("D3"));
	}

	public static void Pause(bool paused = true) {
		Paused = paused;
		Running = !paused;

		Cursor.visible = paused;
	}

	/// <summary>
	/// Activates the next team in the cycle.
	/// </summary>
	public static void ActivateNextTeam() {
		int prevTeam = currentTeam;
		currentTeam++;
		if (currentTeam >= teams.Length) currentTeam = 0;

		teams[prevTeam].takingTurn = false;
		GameEvent e = new GameEvent();
		e.eventTeam = teams[prevTeam];
		e.eventType = GameEventType.TurnEnd;
		GameManager.SendGameEvent (e);

		if (teams[currentTeam].teamType == TeamType.AI) {
			FollowMouse.Active = false;
		} else {
			FollowMouse.Active = true;
		}
	}
	#endregion

	#region FX/Helpers
	/// <summary>
	/// Called to complete current round.
	/// </summary>
	/// <param name="victory">If set to <c>true</c> victory.</param>
	private static void LevelComplete(bool victory) {
		Log ("Player has " + (victory ? "won" : "lost"));
		Debug.LogWarning("Round completed in: " + TimeElapsed.ToString());
		Instance.StartCoroutine(LevelCompleteRoutine(victory));
	}

	static IEnumerator LevelCompleteRoutine(bool victory) {
		// do some checks for level being completed
		if (victory) {
			AudioManager.PlayVictory();
			Pause();

			// short delay for audio to complete
			yield return new WaitForSeconds(0.75f);
			Instance.StartCoroutine(ShowVictoryScreen());
		} else {
			AudioManager.PlayFail();
			Pause();
			Instance.StartCoroutine(ShowDeathScreen());
		}
		// TESTING in AI-only mode (num AI teams == num teams)
		/*yield return new WaitForSeconds(5f);
		Restart();
		VictoryManager.Hide();
		LossManager.Hide();*/
		// END TEST
		yield return null;
	}

	static IEnumerator ShowVictoryScreen() {
		yield return new WaitForSeconds(2f);
		VictoryManager.Show();
		yield return null;
	}

	static IEnumerator ShowDeathScreen() {
		yield return new WaitForSeconds(2f);
		LossManager.Show();
		yield return null;
	}

	/// <summary>
	/// Restart round.
	/// </summary>
	public static void Restart() {
		Instance.StartCoroutine(Restarting());
	}

	static IEnumerator Restarting() {
		levelReady = false;
		Pause();
		ResetRoundTime();
		AbortingGame();

		LoadingUtil.BeginLoad();
		yield return new WaitForSeconds(1f);
		Init();
	}

	/// <summary>
	/// Called when the game is being aborted or restarted.
	/// </summary>
	public static void AbortingGame() {
		// anything required when closing the active game session
		entities.Clear();
		// remove previous team objects
		if (teams != null) {
			for (int i = 0; i < teams.Length; i++) {
				if (teams[i] == null) continue;
				for (int j = 0; j < teams[i].Entities.Count; j++) {
					if (teams[i].Entities[j] != null) {
						Destroy(teams[i].Entities[j].gameObject);
					}
				}
				Destroy(teams[i].gameObject);
			}
		}

		Instance.gameCanvas.ClearStatusPanels();
	}

	/// <summary>
	/// Gets the enemies of provided unit.
	/// </summary>
	/// <returns>The enemies.</returns>
	/// <param name="unit">Unit.</param>
	public static Entity[] GetEnemies(Entity unit) {
		int teamID = unit.Team.TeamID;
		List<Entity> enemies = new List<Entity>();

		for (int i = 0; i < teams.Length; i++) {
			if (teams[i].TeamID != teamID) {
				enemies.AddRange(teams[i].Entities);
			}
		}

		if (enemies.Count > 0) {
			return enemies.ToArray();
		}

		return null;
	}

	/// <summary>
	/// Gets the enemies in range of unit.
	/// </summary>
	/// <returns>The enemies in range.</returns>
	/// <param name="unit">Unit.</param>
	/// <param name="range">Range.</param>
	public static Entity[] GetEnemiesInRange(Entity unit, float range) {
		Entity[] enemies = GetEnemies(unit);
		List<Entity> eList = new List<Entity>();

		for (int i = 0; i < enemies.Length; i++) {
			if (Toolbox.DistanceCheck(unit.Position, enemies[i].Position, range)) {
				eList.Add(enemies[i]);
			}
		}

		if (eList.Count > 0)
			return eList.ToArray();

		return null;
	}

	/// <summary>
	/// Register the specified unit at index.
	/// </summary>
	/// <param name="unit">Unit.</param>
	/// <param name="ndx">Ndx.</param>
	public static void Register(Entity unit, int ndx = -1) {
		if (entities == null) entities = new List<Entity>();
		entities.Add(unit);

		// create character UI panel
		if (ndx >= 0) {
			Instance.gameCanvas.CreateEntityPanel(unit, ndx);
		}
	}

	/// <summary>
	/// Averaged team position.
	/// </summary>
	/// <returns>The center position.</returns>
	/// <param name="u">U.</param>
	public static Vector3 TeamCenterPosition (Entity u) {
		Vector3 c = Vector3.zero;
		int added = 0;
		for (int i = 0; i < entities.Count; i++) {
			if (entities[i] == u || entities[i].Team != u.Team) continue;

			added++;
			c += entities[i].Position;
		}

		if (added > 0) c = c*(float)(1/added);
		else return Vector3.zero;

		return c;
	}

	/// <summary>
	/// Determines if target entity is visible by player team.
	/// </summary>
	/// <returns><c>true</c> if is visible by player; otherwise, <c>false</c>.</returns>
	/// <param name="e">Entity.</param>
	public static bool IsVisibleByPlayer(Entity e) {
		bool vis = false, playerExists = false;

		for (int i = 0; i < teams.Length; i++) {
			if (teams[i] == null) continue;

			if (teams[i].teamType == TeamType.Human) {
				playerExists = true;
				break;
			}
		}

		if (playerExists) {
			for (int i = 0; i < entities.Count; i++) {
				if (!entities[i].isAlive || entities[i].Team == e.Team) continue;

				if (entities[i].Senses.CanSee(e.gameObject)) {
					vis = true;
					break;
				}
			}
		} else {
			vis = true;
		}

		if (!vis) {
			RevealedComponent comp = e.gameObject.GetComponent<RevealedComponent>();
			if (comp != null) {
				vis = true;
			}
		}

		return vis;
	}
	#endregion

	#region Initialization
	/// <summary>
	/// Called when the game scene is loaded.
	/// </summary>
	private void OnLoadGame() {
		gameCanvas = FindObjectOfType<GameCanvas>();
		waitCanvas = FindObjectOfType<WaitCanvas>();
		waitCanvas.Show(false);
		gameCanvas.ShowPanels(false);
		Init();
	}

	/// <summary>
	/// Initializes a round by creating teams.
	/// </summary>
	public static void Init() {
		Instance.StartCoroutine(Initializing());
	}

	static IEnumerator Initializing() {
		if (Instance.numAITeams > Instance.numTeams) {
			Debug.LogError ("Cannot have more AI teams than there are total teams.");
			yield break;
		}

		spawns = FindObjectsOfType<SpawnPoint>();
		if (spawns.Length < Instance.numTeams || spawns.Length < 1) {
			Debug.LogError ("Not enough spawn points for this many teams.");
			yield break;
		}

		// remove previous team objects
		if (teams != null) {
			for (int i = 0; i < teams.Length; i++) {
				if (teams[i] == null) continue;
				for (int j = 0; j < teams[i].Entities.Count; j++) {
					if (teams[i].Entities[j] != null) {
						Destroy(teams[i].Entities[j].gameObject);
					}
				}
				Destroy(teams[i].gameObject);
			}
		}

		// create and assign teams
		teams = new TeamManager[Instance.numTeams];
		for (int x = 0; x < Instance.numTeams; x++) {
			TeamManager teamManager = null;
			GameObject team = null;

			if (x >= (Instance.numTeams-Instance.numAITeams)) {
				// this is for an AI team
				team = (GameObject)Instantiate (Instance.aiTeamManager);
			} else {
				// this is for our player's team
				team = (GameObject)Instantiate (Instance.humanTeamManager);
				team.name = "Squad Team " + x.ToString();

				Instance.gameCanvas.ShowCharacterPanel();
			}

			teamManager = team.GetComponent<TeamManager>();
			if ((x+1)%2 == 0) {
				teamManager.TeamFaction = TeamFaction.Psycho;
			}
			teamManager.TeamID = x;
			teams[x] = teamManager;

			teamManager.Initialize();
			yield return null;
		}

		// set the first team to a valid option
		currentTeam = 0;

		Instance.BeginLevel();
		yield return null;
	}

	/// <summary>
	/// Begins the round.
	/// </summary>
	/// <param name="isReset">If set to <c>true</c> is reset.</param>
	public void BeginLevel(bool isReset = false) {
		StartCoroutine(LevelCreation(isReset));
	}

	IEnumerator LevelCreation(bool isReset) {
		levelReady = false;
		yield return null;

		// reset round time
		ResetRoundTime();
		yield return new WaitForEndOfFrame();

		AudioManager.PlayGameMusic((int)difficulty);

		Log("Finished generating level.");
		levelReady = true;
		LoadingUtil.EndLoad();
		Pause(false);
		WaitCanvas.Show(false);
		StartRoundTime();

		yield return null;
		// level started
	}

	/// <summary>
	/// Update cycle of round.
	/// </summary>
	void GameUpdate() {
		if (!levelReady || !Running) return;

		// let our current team know to go
		if (!teams[currentTeam].takingTurn) {
			teams[currentTeam].StartTurn();

			GameEvent e = new GameEvent();
			e.eventTeam = teams[currentTeam];
			e.eventType = GameEventType.TurnStart;
			SendGameEvent(e);
		}

		// human input
		if (teams[currentTeam].teamType == TeamType.Human) {
			if (cInput.GetKeyDown("Unit 1")) {
				EntitySelected(0);
			} else if (cInput.GetKeyDown("Unit 2")) {
				EntitySelected(1);
			} else if (cInput.GetKeyDown("Unit 3")) {
				EntitySelected(2);
			} else if (cInput.GetKeyDown("Unit 4")) {
				EntitySelected(3);
			} else if (cInput.GetKeyDown("Move")) {
				MoveSelected();
			} else if (cInput.GetKeyDown("Attack")) {
				AttackSelected();
			} else if (cInput.GetKeyDown("Special")) {
				SpecialSelected();
			} else if (cInput.GetKeyDown("Skip Unit")) {
				SkipSelected();
			} else if (cInput.GetKeyDown("Reload")) {
				ReloadSelected();
			} else if (cInput.GetKeyDown("End Turn")) {
				EndTurnSelected();
			}
		}
	}
	#endregion

	#region Unit Control/UI Handlers
	public static void EntitySelected(int ndx) {
		if (teams[currentTeam].teamType == TeamType.Human) {
			teams[currentTeam].SelectEntity(ndx);
			FollowMouse.SetMode(true);
		}
	}
	public static void MoveSelected() {
		teams[currentTeam].MoveSelect();
	}
	public static void AttackSelected() {
		teams[currentTeam].AttackSelect();
	}
	public static void SpecialSelected() {
		teams[currentTeam].SpecialSelect();
	}
	public static void SkipSelected() {
		teams[currentTeam].SkipSelect();
	}
	public static void ReloadSelected() {
		teams[currentTeam].ReloadSelect();
	}
	public static void EndTurnSelected() {
		teams[currentTeam].CurrentPhase = TurnPhase.EndTurn;
	}

	public static void LeftClickReceived() {
		teams[currentTeam].LeftClick();
	}
	#endregion

	#region Accessors
	public static bool Paused {
		get { return Instance.isPaused; }
		set { Instance.isPaused = value; }
	}

	public static bool Running {
		get { return Instance.gameRunning; }
		set { Instance.gameRunning = value; }
	}

	public static int Seed {
		get { return seed; }
		set { seed = value; }
	}

	public static float TimeElapsed {
		get { return Instance.roundTimeElapsed; }
		set { Instance.roundTimeElapsed = value; }
	}

	public static Entity[] Entities {
		get { return entities.ToArray(); }
	}

	public static GameCanvas GameCanvas {
		get { return Instance.gameCanvas; }
	}

	public static GameObject ActionMenu {
		get { return Instance.gameCanvas.actionPanel; }
	}

	public static WaitCanvas WaitCanvas {
		get { return Instance.waitCanvas; }
	}
	#endregion
}

#region Event Classes
public enum GameEventType {
	TurnStart = 0,
	TurnEnd,
	TeamWon,
	TeamLose
}

public struct GameEvent {
	public TeamManager eventTeam;
	public GameEventType eventType;
}

public enum EntityEventType {
	NONE = 0,
	Death,
	Damaged,
	Attacking,
	AttackComplete,
	Attack,
	Move,
	MoveComplete,
	Selected,
	TurnComplete
}

public class EntityEvent {
	public TeamManager eventTeam = null;		// team responsible for event
	public GameObject eventObject = null;		// object that event occurred to
	public GameObject eventBlame = null;		// object that caused event
	public Vector3 eventPosition = Vector3.zero;// location event occurred
	public float eventTime = 0f;				// time of event
	public EntityEventType eventType = EntityEventType.NONE; // type of event

	// blank ctor
	public EntityEvent() {}
	public EntityEvent(TeamManager team, GameObject what, GameObject responsible, Vector3 pos, float time, EntityEventType etype) {
		eventTeam = team;
		eventObject = what;
		eventBlame = responsible;
		eventPosition = pos;
		eventTime = time;
		eventType = etype;
	}
}
#endregion