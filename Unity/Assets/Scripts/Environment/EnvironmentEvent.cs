using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnvironmentEventType {
	DETONATION,
	DEATH
}

public class EnvironmentEvent {
	public EnvironmentEventType eventType = EnvironmentEventType.DETONATION;
	public GameObject eventObject = null;
	public float eventTime = 0f;
	public Vector3 eventPosition = Vector3.zero;

	public EnvironmentEvent() {}
}

public class EnvironmentEventManager : Singleton<EnvironmentEventManager> {
	[HideInInspector]
	public List<EnvironmentEvent> events = new List<EnvironmentEvent>();

	public delegate void EnvironmentEventHandler (EnvironmentEvent e);
	public static event EnvironmentEventHandler OnEvent;

	void OnEnable() {
		OnEvent += EventLogger;
	}

	void OnDisable() {
		OnEvent -= EventLogger;
	}

	void EventLogger (EnvironmentEvent e) {
		events.Add (e);

		Debug.Log ("[Environment Event] " + (e.eventObject != null ? e.eventObject.name : "Unknown") + " : " + e.eventType.ToString() + " -> " + e.eventPosition.ToString() + " @ " + e.eventTime.ToString());
	}

	public static void SendEvent(EnvironmentEvent e) {
		OnEvent(e);
	}

	public static void PrintAllEvents() {
		EnvironmentEvent e;

		for (int i = 0; i < Instance.events.Count; i++) {
			e = Instance.events[i];

			Debug.Log ("[Environment Event] " + (e.eventObject != null ? e.eventObject.name : "Unknown") + " : " + e.eventType.ToString() + " -> " + e.eventPosition.ToString() + " @ " + e.eventTime.ToString());
		}
	}
}
