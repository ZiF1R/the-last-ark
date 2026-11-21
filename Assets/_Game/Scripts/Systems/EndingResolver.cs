using System.Collections.Generic;
using System.Linq;

public static class EndingResolver
{
	public static EndingDef Resolve(RunState state, ContentDatabase db)
	{
		List<CardTag> crewTags = state.GetCrewTags();

		EndingDef best = null;
		foreach (EndingDef e in db.endings)
		{
			if (e == null) continue;

			bool matchStats = IsStatsMatch(e, state.survival, state.science, state.morale);
			if (!matchStats) continue;

			bool matchesAllTags = e.requiredTags.All(t => crewTags.Contains(t));
			if (!matchesAllTags) continue;

			bool matchForbiddenTag = crewTags.Any(t => e.forbiddenTags.Contains(t));
			if (matchForbiddenTag) continue;

			bool matchesAllFlags = e.requiredFlags.All(f => state.flags.Contains(f));
			if (!matchesAllFlags) continue;

			bool matchForbiddenFlag = state.flags.Any(f => e.forbiddenFlags.Contains(f));
			if (matchForbiddenFlag) continue;

			if (best == null || e.priority > best.priority)
				best = e;
		}

		return best;
	}

	static bool IsStatsMatch(EndingDef e, int s, int sc, int m)
	{
		bool matchSurvival = IsStatMatch(s, e.minSurvival, e.maxSurvival);
		bool matchScience = IsStatMatch(s, e.minScience, e.maxScience);
		bool matchMorale = IsStatMatch(s, e.minMorale, e.maxMorale);

		return matchSurvival && matchScience && matchMorale;
	}

	static bool IsStatMatch(int value, int min, int max)
	{
		return value >= min && value <= max;
	}
}
