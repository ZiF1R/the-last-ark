using System.Collections;
using TMPro;
using UnityEngine;

public static class Typewriter
{
	public static IEnumerator Play(TMP_Text target, float charsPerSecond, float maxDuration = 0f)
	{
		if (target == null) yield break;

		target.maxVisibleCharacters = int.MaxValue;
		target.ForceMeshUpdate();

		int total = target.textInfo.characterCount;
		if (total <= 0) yield break;

		target.maxVisibleCharacters = 0;

		float cps = Mathf.Max(1f, charsPerSecond);
		if (maxDuration > 0f)
			cps = Mathf.Max(cps, total / maxDuration);

		float t = 0f;
		int shown = 0;

		while (shown < total)
		{
			if (target == null) yield break;

			t += Time.unscaledDeltaTime * cps;
			int next = Mathf.Min(total, Mathf.FloorToInt(t));

			if (next != shown)
			{
				shown = next;
				target.maxVisibleCharacters = shown;
			}

			yield return null;
		}

		target.maxVisibleCharacters = total;
	}
}
