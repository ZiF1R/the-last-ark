using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
	public static event Action<List<CardDef>> OfferReady;
	public static event Action<StatDelta> StatsChanged;
	public static event Action<RunState> CrewChanged;
	public static event Action<EventDef> EventTriggered;
	public static event Action<EndingDef, RunState> RunEnded;

	public static void ResetAll()
	{
		OfferReady = null;
		StatsChanged = null;
		CrewChanged = null;
		EventTriggered = null;
		RunEnded = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void ResetOnLoad() => ResetAll();
}
