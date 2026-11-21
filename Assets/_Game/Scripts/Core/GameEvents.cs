using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
	public static event Action<RunState> RunStarted;
	public static event Action<List<CardDef>> OfferReady;
	public static event Action<StatDelta> StatsChanged;
	public static event Action<RunState> CrewChanged;
	public static event Action<EventDef> EventTriggered;
	public static event Action<EndingDef, RunState> RunEnded;

	public static void RaiseRunStarted(RunState state) => RunStarted?.Invoke(state);
	public static void RaiseOfferReady(List<CardDef> offer) => OfferReady?.Invoke(offer);
	public static void RaiseStatsChanged(StatDelta delta) => StatsChanged?.Invoke(delta);
	public static void RaiseCrewChanged(RunState state) => CrewChanged?.Invoke(state);
	public static void RaiseEventTriggered(EventDef ev) => EventTriggered?.Invoke(ev);
	public static void RaiseRunEnded(EndingDef end, RunState state) => RunEnded?.Invoke(end, state);

	public static void ResetAll()
	{
		RunStarted = null;
		OfferReady = null;
		StatsChanged = null;
		CrewChanged = null;
		EventTriggered = null;
		RunEnded = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void ResetOnLoad() => ResetAll();
}
