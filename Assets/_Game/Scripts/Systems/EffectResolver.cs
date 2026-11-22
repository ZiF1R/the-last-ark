using UnityEngine;

public static class EffectResolver
{
	public static StatDelta ApplyCard(CardDef card, RunState state)
	{
		if (!string.IsNullOrEmpty(card.eventOnPickId))
			state.pendingEventId = card.eventOnPickId;

		StatDelta immediate = state.ApplyStats(card.survival, card.science, card.morale);

		if (card.delayed != null && !card.delayed.IsEmpty)
		{
			state.AddPendingEffect(
				card.delayed.survival,
				card.delayed.science,
				card.delayed.morale,
				card.delayed.turns,
				card.id);
		}

		return immediate;
	}
}
