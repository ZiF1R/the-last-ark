using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContentDatabase", menuName = "Scriptable Objects/ContentDatabase")]
public class ContentDatabase : ScriptableObject
{
	public List<CardDef> cards;
	public List<EventDef> events;
	public List<EndingDef> endings;
}
