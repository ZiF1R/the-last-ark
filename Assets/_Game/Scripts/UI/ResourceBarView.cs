using TMPro;
using UnityEngine;

public class ResourceBarView : MonoBehaviour
{
	[SerializeField] TMP_Text survivalText;
	[SerializeField] TMP_Text scienceText;
	[SerializeField] TMP_Text moraleText;
	[SerializeField] TMP_Text turnText;

	RunState _state;

	void OnEnable()
	{
		GameEvents.RunStarted += OnRunStarted;
		GameEvents.StatsChanged += OnStatsChanged;
		GameEvents.OfferReady += OnOfferReady;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= OnRunStarted;
		GameEvents.StatsChanged -= OnStatsChanged;
		GameEvents.OfferReady -= OnOfferReady;
	}

	void OnRunStarted(RunState state) { _state = state; Refresh(); }
	void OnStatsChanged(StatDelta _, RunState state) { _state = state; Refresh(); }
	void OnOfferReady(System.Collections.Generic.List<CardDef> _) => Refresh();

	void Refresh()
	{
		if (_state == null) return;

		survivalText.text = _state.survival.ToString();
		scienceText.text = _state.science.ToString();
		moraleText.text = _state.morale.ToString();

		if (turnText != null)
			turnText.text = $"TURN {_state.turn}   ·   {_state.SlotsLeft} SLOTS LEFT";
			//turnText.text = $"TURN {_state.turn}   ·   {_state.SlotsLeft} SLOTS LEFT";
	}
}
