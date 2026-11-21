using System;
using UnityEngine;

[Serializable]
public class CardDef
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
	public string eventOnPick = "";
	public int weight = 10;
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
