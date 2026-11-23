using System;
using System.Collections.Generic;
using UnityEngine;

public static class SynergySystem
{
	public static List<(SynergyDef rule, StatDelta delta)> Evaluate(RunState state, ContentDatabase db)
	{
		var fired = new List<(SynergyDef, StatDelta)>();
		if (state == null || db == null || db.synergies == null) return fired;

		foreach (SynergyDef rule in db.synergies)
		{
			if (rule == null || string.IsNullOrWhiteSpace(rule.id)) continue;
			if (state.firedSynergies.Contains(rule.id)) continue;
			if (!MatchesCrew(rule, state.crew)) continue;

			state.firedSynergies.Add(rule.id);

			if (!string.IsNullOrWhiteSpace(rule.setFlag))
				state.flags.Add(rule.setFlag);

			StatDelta delta = state.ApplyStats(rule.survival, rule.science, rule.morale);
			fired.Add((rule, delta));
		}

		return fired;
	}

	public static List<SynergyDef> Preview(RunState state, ContentDatabase db, CardDef candidate)
	{
		var result = new List<SynergyDef>();
		if (state == null || db == null || db.synergies == null || candidate == null)
			return result;

		var crew = new List<CardDef>(state.crew) { candidate };

		foreach (SynergyDef rule in db.synergies)
		{
			if (rule == null || string.IsNullOrWhiteSpace(rule.id)) continue;
			if (state.firedSynergies.Contains(rule.id)) continue;
			if (!MatchesCrew(rule, crew)) continue;
			result.Add(rule);
		}

		return result;
	}

	static bool MatchesCrew(SynergyDef rule, List<CardDef> crew)
	{
		if (crew == null) return false;

		if (rule.tagA == rule.tagB)
			return CountTag(crew, rule.tagA) >= Mathf.Max(2, rule.countB);

		int needB = Mathf.Max(1, rule.countB);

		return CountTag(crew, rule.tagA) >= 1
			&& CountTag(crew, rule.tagB) >= needB;
	}

	public static int CountTag(List<CardDef> crew, CardTag tag)
	{
		int n = 0;

		foreach (CardDef c in crew)
			if (c != null && c.tags != null && Array.IndexOf(c.tags, tag) >= 0)
				n++;

		return n;
	}
}
