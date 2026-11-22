using System;
using UnityEngine;

public enum CrewSelector
{
	None,
	ById,
	ByTag,
	ByRole
}

[Serializable]
public class CrewRemoval
{
	public CrewSelector selector = CrewSelector.None;

	[Header("ById")]
	public string cardId;

	[Header("ByTag / ByRole")]
	public CardTag tag;
	public CardRole role;

	public bool IsActive => selector != CrewSelector.None;

	public CardDef FindTarget(RunState state)
	{
		if (state == null || state.crew == null || !IsActive)
			return null;

		for (int i = state.crew.Count - 1; i >= 0; i--)
		{
			CardDef card = state.crew[i];
			if (card == null) continue;

			bool match = selector switch
			{
				CrewSelector.ById => card.id == cardId,
				CrewSelector.ByTag => card.tags != null && Array.IndexOf(card.tags, tag) >= 0,
				CrewSelector.ByRole => card.role == role,
				_ => false
			};

			if (match) return card;
		}

		return null;
	}

	public CardDef Apply(RunState state)
	{
		CardDef target = FindTarget(state);
		if (target == null) return null;

		if (state.TryGetSlotIndices(target, out int start, out int count))
		{
			for (int i = 0; i < count; i++)
				state.sealedSlots.Add(start + i);
		}

		state.crew.Remove(target);
		state.lostCrew.Add(target);

		return target;
	}
}
