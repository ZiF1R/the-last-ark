using System;
using UnityEngine;

[Serializable]
public class SynergyDef
{
	public string id;
	public CardTag tagA;
	public CardTag tagB;
	[Min(1)] public int countB = 1;

	[Header("Effect — fires once per run")]
	public int survival, science, morale;
	public string setFlag;

	[Header("Text")]
	public string text;
}
