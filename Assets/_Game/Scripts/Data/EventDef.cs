using System;
using UnityEngine;

[Serializable]
public class EventDef
{
	public string id, title;
	[TextArea] public string body;
	public string causeLine;

	public EventCondition[] conditions;
	public CardTag[] requireTags;
	public CardTag[] forbidTags;

	public int minTurn = 0;
	public int maxTurn = 99;
	public int priority;
	public int weight = 10; // 0 - never, 10 - usual, 20 - frequent
	public bool once = true;

	public Choice[] choices;
}

public enum ConditionKind { TagPresent, TagAbsent, StatAtLeast, StatAtMost, FlagSet, FlagAbsent }

[Serializable]
public class EventCondition
{
	public ConditionKind kind;
	public CardTag tag; // для TagPresent / TagAbsent
	//public StatType stat; // для StatAtLeast / StatAtMost
	public int value; // порог

	//public bool Evaluate(RunState state) => kind switch
	//{
	//	ConditionKind.TagPresent => state.HasTag(tag),
	//	ConditionKind.TagAbsent => !state.HasTag(tag),
	//	ConditionKind.StatAtLeast => state.GetStat(stat) >= value,
	//	ConditionKind.StatAtMost => state.GetStat(stat) <= value,
	//	ConditionKind.FlagSet => state.flags.Contains(tag.ToString()), // либо отдельное string-поле
	//	ConditionKind.FlagAbsent => !state.flags.Contains(tag.ToString()),
	//	_ => true
	//};
}
