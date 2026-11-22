using System;
using System.Collections.Generic;
using UnityEngine;

public static class SynergySystem
{
	public static List<(SynergyDef rule, StatDelta delta)> Evaluate(RunState state, ContentDatabase db)
	{
		var fired = new List<(SynergyDef, StatDelta)>();
		if (db.synergies == null) return fired;

		foreach (SynergyDef rule in db.synergies)
		{
			if (rule == null || string.IsNullOrEmpty(rule.id)) continue;
			if (state.firedSynergies.Contains(rule.id)) continue;
			if (!Matches(rule, state)) continue;

			state.firedSynergies.Add(rule.id);
			if (!string.IsNullOrEmpty(rule.setFlag)) state.flags.Add(rule.setFlag);

			StatDelta d = state.ApplyStats(rule.survival, rule.science, rule.morale);
			fired.Add((rule, d));
		}

		return fired;
	}

	static bool Matches(SynergyDef rule, RunState state)
	{
		if (rule.tagA == rule.tagB)
			return CountTag(state.crew, rule.tagA) >= Mathf.Max(2, rule.countB);

		return state.HasTag(rule.tagA) && state.HasTag(rule.tagB);
	}

	static int CountTag(List<CardDef> crew, CardTag tag)
	{
		int n = 0;
		foreach (CardDef c in crew)
			if (c != null && c.tags != null && Array.IndexOf(c.tags, tag) >= 0) n++;
		return n;
	}
}
