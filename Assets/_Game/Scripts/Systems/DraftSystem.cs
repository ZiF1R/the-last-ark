using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

public class DraftSystem
{
    public List<CardDef> BuildOffer(RunState state, ContentDatabase db, int count = 3)
	{
		List<CardDef> offer = new();
		List<CardDef> candidates = new();

		foreach (CardDef card in db.cards)
		{
			if (state.crew.Contains(card)) continue;
			if (card.slotCost > state.SlotsLeft) continue;
			candidates.Add(card);
		}

		if (candidates.Count <= count) return candidates;

		for (int i = 0; i < count; i++)
		{
			//TODO: after first roll pick different roles

			CardDef candidate = WeightedPicker.PickWeighted(candidates);
			offer.Add(candidate);
			candidates.Remove(candidate);
		}

		return offer;
	}

	//public static CardDef PickWeighted(List<CardDef> candidates)
	//{
	//	int totalCardsWeight = candidates.Sum(c => c.weight);

	//	if (totalCardsWeight <= 0)
	//	{
	//		int i = Random.Range(0, candidates.Count);
	//		return candidates[i];
	//	}

	//	int roll = Random.Range(0, totalCardsWeight);
	//	foreach (CardDef card in candidates)
	//	{
	//		roll -= card.weight;

	//		if (roll < 0)
	//			return card;
	//	}

	//	return candidates[^1]; 
	//}
}
