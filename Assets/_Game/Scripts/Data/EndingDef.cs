using System;
using UnityEngine;

[Serializable]
public class EndingDef
{
	public string id;
	public string title;
	[TextArea] public string body;
	public int priority;
	public int minSurvival = 0, minScience = 0, minMorale = 0; // 0..10
	public int maxSurvival = 10, maxScience = 10, maxMorale = 10; // 0..10
	public string[] requiredFlags, forbiddenFlags;
	public CardTag[] requiredTags;
	public CardTag[] forbiddenTags;
}
