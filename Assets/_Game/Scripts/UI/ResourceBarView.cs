using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBarView : MonoBehaviour
{
	[SerializeField] TMP_Text survivalText;
	[SerializeField] TMP_Text scienceText;
	[SerializeField] TMP_Text moraleText;
	[SerializeField] TMP_Text turnText;
	[SerializeField] TMP_Text pendingText;

	RunState _state;

	void OnEnable()
	{
		GameEvents.RunStarted += OnRunStarted;
		GameEvents.StatsChanged += OnStatsChanged;
		GameEvents.CrewChanged += OnCrewChanged;
		GameEvents.OfferReady += OnOfferReady;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= OnRunStarted;
		GameEvents.StatsChanged -= OnStatsChanged;
		GameEvents.CrewChanged -= OnCrewChanged;
		GameEvents.OfferReady -= OnOfferReady;
	}

	void OnRunStarted(RunState state) { _state = state; Refresh(); }
	void OnStatsChanged(StatDelta _, RunState state) { _state = state; Refresh(); }
	void OnCrewChanged(RunState state) { _state = state; Refresh(); }
	void OnOfferReady(List<CardDef> _) => Refresh();

	void Refresh()
	{
		if (_state == null) return;

		survivalText.text = _state.survival.ToString();
		scienceText.text = _state.science.ToString();
		moraleText.text = _state.morale.ToString();

		if (turnText != null)
			turnText.text = $"TURN {_state.turn}   ·   {_state.SlotsLeft} SLOTS LEFT";

		//if (pendingText != null)
		//	pendingText.text = BuildPending(_state);
	}

	static string BuildPending(RunState state)
	{
		if (state.pendingEffects == null || state.pendingEffects.Count == 0)
			return "";

		var parts = new List<string>();

		foreach (PendingEffect e in state.pendingEffects)
		{
			if (e == null || e.IsEmpty) continue;

			string when = e.turnsLeft == 1 ? "NEXT TURN" : $"IN {e.turnsLeft} TURNS";
			var stats = new List<string>();

			AddPendingStat(stats, e.survival, "SURV");
			AddPendingStat(stats, e.science, "SCI");
			AddPendingStat(stats, e.morale, "MOR");

			if (stats.Count > 0)
				parts.Add($"{when}: {string.Join(", ", stats)}");
		}

		return string.Join("   ·   ", parts);
	}

	static void AddPendingStat(List<string> parts, int v, string label)
	{
		if (v == 0) return;
		parts.Add($"{(v > 0 ? "+" : "")}{v} {label}");
	}
}
