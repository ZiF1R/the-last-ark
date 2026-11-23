using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunState
{
	public int survival = 7, science = 5, morale = 5;
	public int turn;
	public List<CardDef> crew = new();
	public List<CardDef> leftBehind = new();
	public List<CardDef> lostCrew = new();
	public readonly HashSet<int> sealedSlots = new();
	public HashSet<string> flags = new();
	public HashSet<string> usedEventIds = new();
	public List<PendingEffect> pendingEffects = new();
	public readonly HashSet<string> firedSynergies = new();

	public int SlotsLeft => Mathf.Max(
		0,
		MAX_SLOTS
		- crew.Sum(c => c != null ? c.slotCost : 0)
		- sealedSlots.Count
	);
	public bool IsDead => survival <= 0 || science <= 0 || morale <= 0;
	public bool IsFull => SlotsLeft <= 0;
	public string pendingEventId;

	public const int MAX_SLOTS = 6;

	public StatDelta ApplyStats(int s, int sc, int m)
	{
		int s0 = survival, sc0 = science, m0 = morale;

		survival = Mathf.Clamp(survival + s, 0, 10);
		science = Mathf.Clamp(science + sc, 0, 10);
		morale = Mathf.Clamp(morale + m, 0, 10);

		return new (
			survival - s0,
			science - sc0,
			morale - m0
		);
	}

	public void AddPendingEffect(int s, int sc, int m, int turns, string sourceId)
	{
		if (s == 0 && sc == 0 && m == 0) return;

		pendingEffects.Add(new(s, sc, m, Mathf.Max(1, turns), sourceId));
	}

	public StatDelta TickPendingEffects()
	{
		StatDelta total = new(0, 0, 0);

		for (int i = pendingEffects.Count - 1; i >= 0; i--)
		{
			PendingEffect e = pendingEffects[i];

			if (e == null)
			{
				pendingEffects.RemoveAt(i);
				continue;
			}

			e.turnsLeft--;

			if (e.turnsLeft > 0) continue;

			StatDelta applied = ApplyStats(e.survival, e.science, e.morale);

			total.survival += applied.survival;
			total.science += applied.science;
			total.morale += applied.morale;

			pendingEffects.RemoveAt(i);
		}

		return total;
	}

	public bool TryGetSlotIndices(CardDef card, out int start, out int count)
	{
		start = -1;
		count = 0;

		if (card == null) return false;

		int slot = 0;

		foreach (CardDef c in crew)
		{
			if (c == null) continue;

			while (sealedSlots.Contains(slot))
				slot++;

			if (ReferenceEquals(c, card))
			{
				start = slot;
				count = c.slotCost;
				return true;
			}

			slot += c.slotCost;
		}

		return false;
	}

	public StatDelta GetPassiveDelta()
	{
		int s = 0, sc = 0, m = 0;

		foreach (CardDef card in crew)
		{
			int period = Mathf.Max(1, card.passivePeriod);
			if (turn % period != 0) continue;

			s += card.passiveSurvival;
			sc += card.passiveScience;
			m += card.passiveMorale;
		}

		return new StatDelta(s, sc, m);
	}

	public List<CardTag> GetCrewTags()
	{
		List<CardTag> tags = new();

		foreach (CardDef card in crew)
		{
			if (card == null || card.tags == null) continue;
			tags.AddRange(card.tags);
		}

		return tags;
	}

	public bool HasTag(CardTag tag)
	{
		foreach (CardDef card in crew)
			if (card.tags != null && card.tags.Contains(tag))
				return true;

		return false;
	}

	public bool HasRole(CardRole role)
	{
		foreach (CardDef card in crew)
			if (card.role == role)
				return true;

		return false;
	}

	public bool HasFlag(string flag)
		=> !string.IsNullOrEmpty(flag) && flags.Contains(flag);

	public int GetStat(StatKind stat) => stat switch
	{
		StatKind.Survival => survival,
		StatKind.Science => science,
		StatKind.Morale => morale,
		_ => 0
	};
}
