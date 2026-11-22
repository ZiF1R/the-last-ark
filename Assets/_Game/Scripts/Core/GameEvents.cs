using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
	public static event Action<RunState> RunStarted;
	public static event Action<List<CardDef>> OfferReady;
	public static event Action<StatDelta, RunState> StatsChanged;
	public static event Action<RunState> CrewChanged;
	public static event Action<SynergyDef, StatDelta, RunState> SynergyFired;
	public static event Action<EventDef, RunState> EventTriggered;
	public static event Action<Choice, CardDef> EventResolved;
	public static event Action<EndingDef, RunState> RunEnded;

	public static void RaiseRunStarted(RunState state) => RunStarted?.Invoke(state);
	public static void RaiseOfferReady(List<CardDef> offer) => OfferReady?.Invoke(offer);
	public static void RaiseStatsChanged(StatDelta delta, RunState state) => StatsChanged?.Invoke(delta, state);
	public static void RaiseCrewChanged(RunState state) => CrewChanged?.Invoke(state);
	public static void RaiseSynergyFired(SynergyDef r, StatDelta d, RunState s) => SynergyFired?.Invoke(r, d, s);
	public static void RaiseEventTriggered(EventDef ev, RunState state) => EventTriggered?.Invoke(ev, state);
	public static void RaiseEventResolved(Choice ch, CardDef lost) => EventResolved?.Invoke(ch, lost);
	public static void RaiseRunEnded(EndingDef end, RunState state) => RunEnded?.Invoke(end, state);

	public static void ResetAll()
	{
		RunStarted = null;
		OfferReady = null;
		StatsChanged = null;
		CrewChanged = null;
		EventTriggered = null;
		EventResolved = null;
		SynergyFired = null;
		RunEnded = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void ResetOnLoad() => ResetAll();
}
