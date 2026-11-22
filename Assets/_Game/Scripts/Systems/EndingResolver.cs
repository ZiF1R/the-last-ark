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

			if (!HasAllTags(e, crewTags)) continue;
			if (HasForbiddenTag(e, crewTags)) continue;
			if (!HasAllFlags(e, state.flags)) continue;
			if (HasForbiddenFlag(e, state.flags)) continue;

			if (best == null || e.priority > best.priority)
				best = e;
		}

		return best;
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
		bool matchScience = IsStatMatch(s, e.minScience, e.maxScience);
		bool matchMorale = IsStatMatch(s, e.minMorale, e.maxMorale);

		return matchSurvival && matchScience && matchMorale;
	}

	static bool IsStatMatch(int value, int min, int max)
	{
		return value >= min && value <= max;
	}
}
