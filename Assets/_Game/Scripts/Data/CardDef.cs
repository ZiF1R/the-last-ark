using System;
using UnityEngine;

[Serializable]
public class CardDef : IWeighted
{
	public string id;
	public string displayName;
	public CardRole role;
	[TextArea] public string quote;
	public CardTag[] tags;
	public int slotCost = 1;
	public int survival;
	public int science;
	public int morale;

	[Header("Passive (applied at the start of each turn)")]
	public int passiveSurvival, passiveScience, passiveMorale;

	[Space]
	// 1 - every round, 2 - every second ... etc.
	[Min(1)] public int passivePeriod = 1;

	[Space]

	[Header("Delayed cost (applies N turns after hire)")]
	public DelayedEffect delayed = new();

	[Space]

	public string eventOnPickId;
	[field: SerializeField] public int weight { get; set; } = 10; // 0 - never, 10 - usual, 20 - frequent
	public Sprite icon;
}

public enum CardRole
{
	Biolog,
	Engineer,
	Soldier,
	Medic,
	Child,
	Sick,
	Farmer,
	Mutant,
}

public enum CardTag
{
	Engineer,
	Stubborn,
	Scientist,
	Child,
	Soldier,
	Medic,
	Dreamer,
	Sick,
	Farmer,
	Mutant,
}
