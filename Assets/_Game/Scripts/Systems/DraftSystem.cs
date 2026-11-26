using System.Collections.Generic;
using UnityEngine;

public class DraftSystem
{
    public List<CardDef> BuildOffer(RunState state, ContentDatabase db, int count = 3)
	{
		List<CardDef> offer = new();
		List<CardDef> candidates = new();

		foreach (CardDef card in db.cards)
		{
			if (card == null) continue;
			if (state.crew.Contains(card)) continue;
			if (state.lostCrew.Contains(card)) continue;
			if (card.slotCost > state.SlotsLeft) continue;
			candidates.Add(card);
		}

		if (candidates.Count == 0)
			Debug.LogWarning($"[Draft] Empty offer: turn {state.turn}, slots left {state.SlotsLeft}, " +
				$"crew {state.crew.Count}, sealed {state.sealedSlots.Count}, lost {state.lostCrew.Count}");

		if (candidates.Count <= count) return candidates;

		for (int i = 0; i < count; i++)
		{
			CardDef candidate = WeightedPicker.PickWeighted(candidates);
			offer.Add(candidate);
			candidates.Remove(candidate);
		}

		return offer;
	}
}
