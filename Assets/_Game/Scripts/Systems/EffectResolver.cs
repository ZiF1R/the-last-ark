using UnityEngine;

public class EffectResolver
{
	public StatDelta ApplyCard(CardDef card, RunState state)
	{
		int s0 = state.survival, sc0 = state.science, m0 = state.morale;

		state.Apply(card.survival, card.science, card.morale);

		if (!string.IsNullOrEmpty(card.eventOnPickId))
			state.pendingEventId = card.eventOnPickId;

		return new StatDelta(
			state.survival - s0,
			state.science - sc0,
			state.morale - m0
		);
	}
}
