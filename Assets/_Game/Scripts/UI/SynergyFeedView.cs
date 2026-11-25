using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SynergyFeedView : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] TMP_Text line;
	[SerializeField] CanvasGroup group;

	[Header("Timing")]
	[SerializeField] float fadeIn = 0.15f;
	[SerializeField] float hold = 4f;
	[SerializeField] float holdPerExtraLine = 1f;
	[SerializeField] float fadeOut = 0.35f;

	[Header("Colors")]
	[SerializeField] Color neutral = new Color(0.54f, 0.56f, 0.55f);
	[SerializeField] Color bad = new Color(0.79f, 0.34f, 0.29f);
	[SerializeField] Color flavor = new Color(0.85f, 0.82f, 0.77f);

	readonly Queue<string> _queue = new();
	Coroutine _runner;

	void Awake()
	{
		if (line == null) line = GetComponentInChildren<TMP_Text>(true);
		if (group == null) group = GetComponent<CanvasGroup>();
		if (group == null) group = gameObject.AddComponent<CanvasGroup>();

		group.alpha = 0f;
		group.interactable = false;
		group.blocksRaycasts = false;

		if (line != null)
			line.raycastTarget = false;
	}

	void OnEnable()
	{
		GameEvents.SynergyFired += OnSynergyFired;
		GameEvents.RunStarted += OnRunStarted;
	}

	void OnDisable()
	{
		GameEvents.SynergyFired -= OnSynergyFired;
		GameEvents.RunStarted -= OnRunStarted;

		StopRunner();
		_queue.Clear();

		if (group != null) group.alpha = 0f;
	}

	void OnRunStarted(RunState _)
	{
		StopRunner();
		_queue.Clear();
		if (group != null) group.alpha = 0f;
	}

	void OnSynergyFired(SynergyDef rule, StatDelta delta, RunState state)
	{
		if (rule == null) return;

		_queue.Enqueue(BuildLine(rule, delta));

		if (_runner == null && isActiveAndEnabled)
			_runner = StartCoroutine(ShowBatch());
	}

	IEnumerator ShowBatch()
	{
		yield return null;

		int count = 0;
		var sb = new StringBuilder();

		while (_queue.Count > 0)
		{
			if (count > 0) sb.Append("\n\n");
			sb.Append(_queue.Dequeue());
			count++;
		}

		if (count == 0)
		{
			_runner = null;
			yield break;
		}

		if (line != null) line.text = sb.ToString();

		yield return Fade(0f, 1f, fadeIn);
		yield return new WaitForSecondsRealtime(hold + holdPerExtraLine * Mathf.Max(0, count - 1));
		yield return Fade(1f, 0f, fadeOut);

		_runner = null;

		if (_queue.Count > 0 && isActiveAndEnabled)
			_runner = StartCoroutine(ShowBatch());
	}

	IEnumerator Fade(float from, float to, float duration)
	{
		if (group == null) yield break;

		if (duration <= 0f)
		{
			group.alpha = to;
			yield break;
		}

		float t = 0f;

		while (t < duration)
		{
			t += Time.unscaledDeltaTime;
			group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
			yield return null;
		}

		group.alpha = to;
	}

	void StopRunner()
	{
		if (_runner == null) return;

		StopCoroutine(_runner);
		_runner = null;
	}

	string BuildLine(SynergyDef rule, StatDelta delta)
	{
		var sb = new StringBuilder();

		string accent = UITheme.Html(rule.IsNegative ? UITheme.Danger : UITheme.Muted);

		sb.Append($"<size={UITheme.FsMicro}><color=#{accent}>SYNERGY</color></size>\n");
		sb.Append(TagLabel(rule.tagA));

		if (rule.tagB != rule.tagA)
		{
			sb.Append($" + {TagLabel(rule.tagB)}");

			int needB = Mathf.Max(1, rule.countB);
			if (needB > 1) sb.Append($" × {needB}");
		}
		else
		{
			sb.Append($" × {Mathf.Max(2, rule.countB)}");
		}

		sb.Append($"\n<size={UITheme.FsBody}>{BuildDelta(delta)}</size>");

		return sb.ToString();
	}

	static string BuildDelta(StatDelta d)
	{
		if (d == null) return "";

		var parts = new List<string>();

		AddChip(parts, d.survival, "SURVIVAL", UITheme.Surv);
		AddChip(parts, d.science, "SCIENCE", UITheme.Sci);
		AddChip(parts, d.morale, "MORALE", UITheme.Mor);

		if (parts.Count == 0)
			return $"<color=#{UITheme.Html(UITheme.Muted)}>no stat change</color>";

		return string.Join("   ", parts);
	}

	static void AddChip(List<string> parts, int v, string label, Color color)
	{
		if (v == 0) return;
		parts.Add($"<color=#{UITheme.Html(color)}>{(v > 0 ? "+" : "")}{v} {label}</color>");
	}

	static string TagLabel(CardTag tag) => tag.ToString().ToUpperInvariant();
}
