using System;
using UnityEngine;

[Serializable]
public class Choice
{
	public string label;
	[TextArea] public string result;
	public int survival, science, morale;
	public string setFlag;

	[Header("Availability")]
	public StateCondition[] requirements;
	public string lockHint;

	[Header("Consequence")]
	public CrewRemoval removal = new();

	public bool IsAvailable(RunState state)
	{
		if (requirements == null || requirements.Length == 0) return true;
		foreach (StateCondition r in requirements)
		{
			if (r == null) continue;
			if (!r.Evaluate(state)) return false;
		}
		return true;
	}

	public string GetLockHint(RunState state)
	{
		if (!string.IsNullOrEmpty(lockHint)) return lockHint;
		if (requirements == null) return "";

		foreach (StateCondition r in requirements)
		{
			if (r == null) continue;
			if (!r.Evaluate(state)) return r.Describe();
		}
		return "";
	}
}
