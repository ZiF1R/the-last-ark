using System;
using UnityEngine;

[Serializable]
public class EventDef
{
	public string id, title;
	[TextArea] public string body;
	public CardTag[] requireTags;
	public CardTag[] forbidTags;
	public int minTurn = 0;
	public int maxTurn = 99;
	public Choice[] choices;
}
