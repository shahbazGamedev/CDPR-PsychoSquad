using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script attached to game camera that interfaces with game world
/// for control/manipulation/status of units, and handles camera movement.
/// </summary>
public class FollowMouse : Singleton<FollowMouse> {
	public enum IconType {
		Select = 0,
		Move = 1,
		Attack = 2
	}

	public enum CamMode {
		Follow,
		Move
	}

	public static Transform tr;
	public static Entity target;
	public static bool followTarget = false;
	private static Vector3 targetPos;

	public GameObject pointerObject;
	public GameObject cursorQuad;
	public GameObject northQuad;
	public GameObject southQuad;
	public GameObject eastQuad;
	public GameObject westQuad;
	public Sprite selectIcon;
	public Sprite moveIcon;
	public Sprite attackIcon;
	public Sprite quarterCoverIcon;
	public Sprite halfCoverIcon;
	public Sprite threeQuarterCoverIcon;
	public Sprite fullCoverIcon;
	public Vector2 touchOffset = new Vector2(-50f, 50f);		// offset of cursor for touch purposes so cursor remains visible
	public Vector3 followOffset = new Vector3(0f, 20f, 10f);	// distance for camera to follow target
	public Vector3 pointerOffset = new Vector3(0f, 0.1f, 0f);	// height for pointer to sit over terrain
	public float camFollowSpeed = 1f;
	public float camRotateSpeed = 1f;

	private static AutoCover autoCover;
	private static IconType icon = IconType.Move;
	private static Color color = Color.cyan;
	private static Ray ray;
	private static RaycastHit hit;
	private static Vector3 pos;
	private static Image image;
	private static CamMode mode = CamMode.Follow;
	private Vector3 touchPos = Vector3.zero;

	List<GameObject> obstructions = new List<GameObject>();
	public Material obstructionMaterial;

	#region MonoBehaviour Methods
	void Start() {
		image = cursorQuad.GetComponent<Image>();
		autoCover = GetComponentInChildren<AutoCover>();
		tr = transform;

		if (image == null) {
			enabled = false;
		}

		#if !UNITY_EDITOR && !UNITY_STANDALONE
		Active = false;
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		if (!GameManager.Running) return;

		if (target != null) {
			targetPos = target.Position;
		}

		CheckForTargetObstructions();

		if (EventSystem.current.IsPointerOverGameObject()) {
			Cursor.visible = true;
			touchPos = Toolbox.InvalidV3;
			return;
		} else {
			Cursor.visible = false;
		}

		bool updatePos = false;
		#if UNITY_EDITOR || UNITY_STANDALONE
		ray = Camera.main.ScreenPointToRay (Input.mousePosition/*new Vector3(Mathf.Clamp(Input.mousePosition.x+touchOffset.x, 0f, Screen.width),
														Mathf.Clamp(Input.mousePosition.y+touchOffset.y, 0f, Screen.height),
														0f)*/);
		if (Physics.Raycast (ray, out hit, 100f, LayerMask.GetMask("Ground"))) {
			pos = Toolbox.SnapToGrid(hit.point);
			pos.y = hit.point.y;
			pointerObject.transform.position = pos+pointerOffset;
			if (!Active) Active = true;

			UpdateCoverIcons();
		} else {
			Active = false;
			pos = Toolbox.InvalidV3;
		}

		if (Input.GetMouseButtonUp(0)) {
			updatePos = true;
		}

		touchPos = GetMoveDirection(Input.mousePosition);
		#else
		touchPos = Toolbox.InvalidV3;
		for (int i = 0; i < Input.touches.Length; i++) {
			if (Input.touches[i].phase == TouchPhase.Began) {
				// activate the pointer
				Active = true;
			} else if (Input.touches[i].phase == TouchPhase.Ended) {
				// deactivate pointer
				Active = false;
				updatePos = true;
				break;
			}

			ray = Camera.main.ScreenPointToRay(new Vector3(Mathf.Clamp(Input.touches[i].position.x+touchOffset.x, 0f, Screen.width), 
															Mathf.Clamp(Input.touches[i].position.y+touchOffset.y, 0f, Screen.height), 
															0f));
			if (Physics.Raycast (ray, out hit, 100f, LayerMask.GetMask("Ground"))) {
				pos = Toolbox.SnapToGrid(hit.point);
				pos.y = hit.point.y;
				pointerObject.transform.position = pos+pointerOffset+new Vector3(touchOffset.x, touchOffset.y, 0f);

				UpdateCoverIcons();
			} else {
				pos = Toolbox.InvalidV3;
			}

			touchPos = GetMoveDirection(Input.touches[i].position);
		}
		#endif

		if (touchPos != Toolbox.InvalidV3) SetMode(false);

		if (updatePos) {
			if (pos != Toolbox.InvalidV3 && touchPos == Toolbox.InvalidV3) {
				GameManager.LeftClickReceived();
			}
		}
	}

	void LateUpdate() {
		if (mode == CamMode.Follow) {
			tr.position = Vector3.Lerp(tr.position, targetPos+followOffset, camFollowSpeed*Time.deltaTime);
			tr.rotation = Quaternion.Lerp(tr.rotation, Quaternion.LookRotation((targetPos-tr.position).normalized), camRotateSpeed*Time.deltaTime);
		} else if (touchPos != Toolbox.InvalidV3) {
			tr.position = Vector3.Lerp(tr.position, tr.position-touchPos, camFollowSpeed*2f*Time.deltaTime);
			tr.rotation = Quaternion.Lerp(tr.rotation, Quaternion.LookRotation(-followOffset.normalized), camRotateSpeed*Time.deltaTime);
		} else {
			tr.rotation = Quaternion.Lerp(tr.rotation, Quaternion.LookRotation(-followOffset.normalized), camRotateSpeed*Time.deltaTime);
		}

		if (pos != Toolbox.InvalidV3) pointerObject.transform.position = pos+pointerOffset;
		else Active = false;
		pointerObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}

	void OnLevelWasLoaded(int i) {
		if (obstructions != null) obstructions.Clear();
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game") {
			Cursor.visible = true;
		}
	}
	#endregion

	#region Helpers
	/// <summary>
	/// Updates the cover icons for the pointer.
	/// </summary>
	void UpdateCoverIcons() {
		if (autoCover == null) return;

		Vector3[] dirs = new Vector3[]{Vector3.forward, Vector3.back, Vector3.right, Vector3.left};
		GameObject[] quads = new GameObject[]{northQuad, southQuad, eastQuad, westQuad};
		Image im;
		float x = 0;

		for (int i = 0; i < dirs.Length; i++) {
			x = autoCover.GetCoverageFromDir(dirs[i]);
			im = quads[i].GetComponent<Image>();

			if (x < 0.25f) {
				im.enabled = false;
			} else if (x < 0.5f) {
				im.enabled = true;
				im.sprite = quarterCoverIcon;
			} else if (x < 0.75f) {
				im.enabled = true;
				im.sprite = halfCoverIcon;
			} else if (x < 1f) {
				im.enabled = true;
				im.sprite = threeQuarterCoverIcon;
			} else {
				im.enabled = true;
				im.sprite = fullCoverIcon;
			}
		}
	}

	/// <summary>
	/// Gets the desired camera move direction.
	/// </summary>
	/// <returns>The move direction.</returns>
	/// <param name="pos">Position.</param>
	Vector3 GetMoveDirection(Vector2 pos) {
		return GetMoveDirection(new Vector3(pos.x, pos.y, 0f));
	}

	/// <summary>
	/// Gets the desired camera move direction.
	/// </summary>
	/// <returns>The move direction.</returns>
	/// <param name="pos">Position.</param>
	Vector3 GetMoveDirection(Vector3 pos) {
		Vector3 moveDir = Vector3.zero;
		float w = (float)Screen.width, h = (float)Screen.height;

		if (pos.x <= (w*0.1f)) {
			moveDir += Vector3.left;
		} else if (pos.x >= w-(w*0.1f)) {
			moveDir += Vector3.right;
		}
		if (pos.y <= (h*0.1f)) {
			moveDir += Vector3.back;
		} else if (pos.y >= h-(h*0.1f)) {
			moveDir += Vector3.forward;
		}

		if (moveDir != Vector3.zero)
			return moveDir;
		else return Toolbox.InvalidV3;
	}

	/// <summary>
	/// Checks for target obstructions between camera and cursor/target transforms.
	/// </summary>
	void CheckForTargetObstructions() {
		List<GameObject> temp = new List<GameObject>();
		RaycastHit[] hit;
		int hitCount = 0;

		// raycast against cursor pos
		Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0f));
		ray = new Ray(center, (PointerPosition-center).normalized);
		hit = Physics.RaycastAll(ray, 100f, LayerMask.GetMask("Obstacle"));
		for (int i = 0; i < hit.Length; i++) {
			temp.Add(hit[i].transform.gameObject);
		}
		hitCount += hit.Length;

		if (target != null) {
			// raycast against target pos
			ray = new Ray(center, (target.Position-center).normalized);
			hit = Physics.RaycastAll(ray, 100f, LayerMask.GetMask("Obstacle"));
			for (int i = 0; i < hit.Length; i++) {
				temp.Add(hit[i].transform.gameObject);
			}
			hitCount += hit.Length;
		}

		// add component and hide obstruction
		for (int i = 0; i < temp.Count; i++) {
			if (obstructions.Contains(temp[i])) continue;
				temp[i].AddComponent<Obstruction>();
		}

		// check for unobstructed objects
		if (obstructions != null) {
			for (int i = 0; i < obstructions.Count; i++) {
				if (temp.Contains(obstructions[i])) continue;

				obstructions[i].GetComponent<Obstruction>().Remove();
			}
		}

		obstructions = temp;
	}
	#endregion

	#region Public Static Methods
	/// <summary>
	/// Sets the cursor icon type.
	/// </summary>
	/// <param name="iconType">Icon type.</param>
	public static void SetIcon(IconType iconType) {
		if (icon == iconType) return;

		icon = iconType;
		if (icon == IconType.Attack) {
			image.sprite = Instance.attackIcon;
		} else if (iconType == IconType.Move){
			image.sprite = Instance.moveIcon;
		} else {
			image.sprite = Instance.selectIcon;
		}
	}

	/// <summary>
	/// Sets the cursor color.
	/// </summary>
	/// <param name="c">C.</param>
	public static void SetColor(Color c) {
		if (color == c) return;

		color = c;
		image.color = c;
	}

	/// <summary>
	/// Sets the camera mode.
	/// </summary>
	/// <param name="follow">If set to <c>true</c> follow the target.</param>
	public static void SetMode(bool follow = true) {
		if (follow) {
			followTarget = true;
			mode = CamMode.Follow;
		} else {
			followTarget = false;
			mode = CamMode.Move;
		}
	}
	#endregion

	#region Public Accessors
	public static bool Active {
		get { return Instance.pointerObject.activeInHierarchy; }
		set { Instance.pointerObject.SetActive(value); }
	}

	public static Vector3 Position {
		get { return tr.position; }
	}

	public static Vector3 PointerPosition {
		get { return Instance.pointerObject.transform.position; }
	}

	public static Material HideMaterial {
		get { return Instance.obstructionMaterial; }

	}
	#endregion
}
