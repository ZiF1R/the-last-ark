using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunState
{
	public int survival = 7, science = 5, morale = 5;
	public int turn;
	public List<CardDef> crew = new();
	public List<CardDef> leftBehind = new();
	public HashSet<string> flags = new();
	public int SlotsLeft => MAX_SLOTS - crew.ToArray().Sum(c => c.slotCost);
	public bool IsDead => survival <= 0 || science <= 0 || morale <= 0;
	public bool IsFull => SlotsLeft <= 0;
	public string pendingEventId;

	public const int MAX_SLOTS = 6;

	public void Apply(int s, int sc, int m)
	{
		survival = Mathf.Clamp(survival + s, 0, 10);
		science = Mathf.Clamp(science + sc, 0, 10);
		morale = Mathf.Clamp(morale + m, 0, 10);
	}

	public List<CardTag> GetCrewTags()
	{
		List<CardTag> tags = new();

		foreach (CardDef card in crew)
		{
			tags.AddRange(card.tags);
		}

		return tags;
	}
}
