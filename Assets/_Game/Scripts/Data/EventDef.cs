using System;
using UnityEngine;

[Serializable]
public class EventDef : IWeighted
{
	public string id;
	public string title;
	[TextArea] public string body;

	public string causeLine;

	[Header("Conditions - all required")]
	public EventCondition[] conditions;

	[Header("When can appear")]
	public int minTurn = 0;
	public int maxTurn = 99;

	[Header("Settings")]
	public int priority;
	public int weight { get; set; } = 10;
	public bool once = true;

	public Choice[] choices;

	public bool Matches(RunState state)
	{
		if (state.turn < minTurn || state.turn > maxTurn) return false;
		if (conditions == null) return true;

		foreach (EventCondition c in conditions)
		{
			if (c == null) continue;
			if (!c.Evaluate(state)) return false;
		}

		return true;
	}
}

public enum ConditionKind
{
	TagPresent,
	RolePresent,
	FlagSet,
	StatAtLeast,
	CrewCountAtLeast,

	TagAbsent,
	RoleAbsent,
	FlagAbsent,
	StatAtMost,
	CrewCountAtMost,
}

public enum StatKind { Survival, Science, Morale }

[Serializable]
public class EventCondition
{
	public ConditionKind kind;

	public CardTag tag;
	public CardRole role;
	public string flag;
	public StatKind stat;
	public int value;

	public bool Evaluate(RunState state) => kind switch
	{
		ConditionKind.TagPresent => state.HasTag(tag),
		ConditionKind.TagAbsent => !state.HasTag(tag),
		ConditionKind.RolePresent => state.HasRole(role),
		ConditionKind.RoleAbsent => !state.HasRole(role),
		ConditionKind.FlagSet => state.HasFlag(flag),
		ConditionKind.FlagAbsent => !state.HasFlag(flag),
		ConditionKind.StatAtLeast => state.GetStat(stat) >= value,
		ConditionKind.StatAtMost => state.GetStat(stat) <= value,
		ConditionKind.CrewCountAtLeast => state.crew.Count >= value,
		ConditionKind.CrewCountAtMost => state.crew.Count <= value,
		_ => true
	};
}
