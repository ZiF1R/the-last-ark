using UnityEngine;

public static class EffectResolver
{
	public static StatDelta ApplyCard(CardDef card, RunState state)
	{
		if (!string.IsNullOrEmpty(card.eventOnPickId))
			state.pendingEventId = card.eventOnPickId;

		return state.ApplyStats(card.survival, card.science, card.morale);
	}
}
