using System;
using UnityEngine;

[Serializable]
public class DelayedEffect
{
	public int survival, science, morale;
	[Min(1)] public int turns = 1;

	public bool IsEmpty => survival == 0 && science == 0 && morale == 0;
}
