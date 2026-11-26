using System.Collections.Generic;

public class DraftSystem
{
    public List<CardDef> BuildOffer(RunState state, ContentDatabase db, int count = 3)
	{
		List<CardDef> offer = new();
		List<CardDef> candidates = new();

		foreach (CardDef card in db.cards)
		{
			if (state.crew.Contains(card)) continue;
			if (state.lostCrew.Contains(card)) continue;

			// if there are only cards with 2 slots cost, empty slot remaining //! fix
			if (card.slotCost > state.SlotsLeft) continue;
			candidates.Add(card);
		}

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
