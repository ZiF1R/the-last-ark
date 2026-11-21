using System;
using UnityEngine;
using static CardDef;

[Serializable]
public class EndingDef
{
	public string id;
	public string title;
	[TextArea] public string body;
	public Priority priority;
	public int minSurvival, minScience, minMorale;
	public int maxSurvival, maxScience, maxMorale;
	public string[] requiredFlags, forbiddenFlags;

	[Serializable]
	public enum Priority
	{
		Low,
		Mid,
		High,
	}
}
