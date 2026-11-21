using System;
using UnityEngine;
using UnityEngine.Events;

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
	public string eventOnPickId;
	public int weight { get; set; } = 10; // 0 - never, 10 - usual, 20 - frequent
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
