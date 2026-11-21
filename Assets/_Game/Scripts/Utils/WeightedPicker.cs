using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WeightedPicker
{
	public static T PickWeighted<T>(IReadOnlyList<T> candidates) where T : IWeighted
	{
		int totalWeight = candidates.Sum(c => c.weight);

		if (totalWeight <= 0)
			return candidates[Random.Range(0, candidates.Count)];

		int roll = Random.Range(0, totalWeight);
		for (int i = 0; i < candidates.Count; i++)
		{
			roll -= candidates[i].weight;
			if (roll < 0)
				return candidates[i];
		}

		return candidates[candidates.Count - 1];
	}
}
