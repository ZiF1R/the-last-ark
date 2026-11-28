using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EndingResolver
{
	public static EndingDef Resolve(RunState state, ContentDatabase db)
	{
		List<CardTag> crewTags = state.GetCrewTags();

		List<EndingDef> best = new();

		foreach (EndingDef e in db.endings)
		{
			if (e == null) continue;

			if (!IsStatsMatch(e, state.survival, state.science, state.morale)) continue;
			if (!HasAllTags(e, crewTags)) continue;
			if (HasForbiddenTag(e, crewTags)) continue;
			if (!HasAllFlags(e, state.flags)) continue;
			if (HasForbiddenFlag(e, state.flags)) continue;

			// Keep every ending that ties for the best priority, then pick one
			// at random. Earned endings are alone at their priority, so they
			// stay deterministic; the death variants share a priority and take
			// turns, so the same stats do not always read the same way twice.
			if (best.Count == 0 || e.priority > best[0].priority)
			{
				best.Clear();
				best.Add(e);
			}
			else if (e.priority == best[0].priority)
			{
				best.Add(e);
			}
		}

		return best.Count == 0 ? null : best[Random.Range(0, best.Count)];
	}

	static bool HasAllTags(EndingDef e, List<CardTag> tags)
	=> e.requiredTags == null || e.requiredTags.All(tags.Contains);
	static bool HasForbiddenTag(EndingDef e, List<CardTag> tags)
		=> e.forbiddenTags != null && e.forbiddenTags.Any(tags.Contains);
	static bool HasAllFlags(EndingDef e, HashSet<string> flags)
	=> e.requiredFlags == null || e.requiredFlags.All(flags.Contains);
	static bool HasForbiddenFlag(EndingDef e, HashSet<string> flags)
		=> e.forbiddenFlags != null && e.forbiddenFlags.Any(flags.Contains);

	static bool IsStatsMatch(EndingDef e, int s, int sc, int m)
	{
		bool matchSurvival = IsStatMatch(s, e.minSurvival, e.maxSurvival);
		bool matchScience = IsStatMatch(sc, e.minScience, e.maxScience);
		bool matchMorale = IsStatMatch(m, e.minMorale, e.maxMorale);

		return matchSurvival && matchScience && matchMorale;
	}

	static bool IsStatMatch(int value, int min, int max)
	{
		return value >= min && value <= max;
	}
}
