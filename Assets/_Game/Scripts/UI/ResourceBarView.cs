using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBarView : MonoBehaviour
{
	[SerializeField] TMP_Text survivalText;
	[SerializeField] TMP_Text scienceText;
	[SerializeField] TMP_Text moraleText;
	[SerializeField] TMP_Text turnText;
	[SerializeField] TMP_Text pendingText;

	[Header("Floating Deltas")]
	[SerializeField] RectTransform deltaLayer;
	//[SerializeField] bool showFloaters = true;
	[SerializeField] float floaterRise = 48f;
	[SerializeField] float floaterDuration = 0.75f;
	[SerializeField] Color gainColor = new Color(0.45f, 0.82f, 0.46f);
	[SerializeField] Color lossColor = new Color(0.90f, 0.38f, 0.33f);


	[Header("Danger Pulse")]
	[SerializeField] int dangerThreshold = 2;
	[SerializeField] Color dangerColor = new Color(0.86f, 0.34f, 0.29f);
	[SerializeField] float pulseSpeed = 2.4f;

	const int Survival = 0;
	const int Science = 1;
	const int Morale = 2;

	readonly TMP_Text[] _texts = new TMP_Text[3];
	readonly Color[] _baseColor = new Color[3];
	readonly Vector3[] _baseScale = new Vector3[3];
	readonly Coroutine[] _pulse = new Coroutine[3];
	readonly Coroutine[] _punch = new Coroutine[3];
	readonly List<TMP_Text> _floaters = new();
	readonly int[] _jitter = new int[3];

	RunState _state;

	void Awake()
	{
		_texts[Survival] = survivalText;
		_texts[Science] = scienceText;
		_texts[Morale] = moraleText;

		for (int i = 0; i < _texts.Length; i++)
		{
			if (_texts[i] == null) continue;
			_baseColor[i] = _texts[i].color;
			_baseScale[i] = _texts[i].transform.localScale;
		}
	}

	void OnEnable()
	{
		GameEvents.RunStarted += OnRunStarted;
		GameEvents.StatsChanged += OnStatsChanged;
		GameEvents.CrewChanged += OnCrewChanged;
		GameEvents.OfferReady += OnOfferReady;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= OnRunStarted;
		GameEvents.StatsChanged -= OnStatsChanged;
		GameEvents.CrewChanged -= OnCrewChanged;
		GameEvents.OfferReady -= OnOfferReady;

		ResetVisuals();
	}

	void OnRunStarted(RunState state)
	{
		ResetVisuals();
		_state = state;
		Refresh();
	}

	void OnStatsChanged(StatDelta delta, RunState state)
	{
		_state = state;
		Refresh();

		if (delta == null) return;

		Punch(Survival, delta.survival);
		Punch(Science, delta.science);
		Punch(Morale, delta.morale);

		SpawnFloater(Survival, delta.survival);
		SpawnFloater(Science, delta.science);
		SpawnFloater(Morale, delta.morale);
	}

	void OnCrewChanged(RunState state)
	{
		_state = state;
		Refresh();
	}

	void OnOfferReady(List<CardDef> _) => Refresh();

	void Refresh()
	{
		if (_state == null) return;

		survivalText.text = _state.survival.ToString();
		scienceText.text = _state.science.ToString();
		moraleText.text = _state.morale.ToString();

		if (turnText != null)
			turnText.text = $"TURN {_state.turn}   ·   {_state.SlotsLeft} SLOTS LEFT";

		RefreshPending();
		RefreshDanger();
	}
	void RefreshPending()
	{
		if (pendingText == null) return;

		string text = BuildPending(_state);
		pendingText.text = text;
		pendingText.gameObject.SetActive(!string.IsNullOrEmpty(text));
	}

	void RefreshDanger()
	{
		SetDanger(Survival, _state.survival);
		SetDanger(Science, _state.science);
		SetDanger(Morale, _state.morale);
	}

	void SetDanger(int index, int value)
	{
		bool danger = value <= dangerThreshold && value > 0;

		if (danger)
		{
			if (_pulse[index] == null && _texts[index] != null)
				_pulse[index] = StartCoroutine(PulseRoutine(index));
		}
		else if (_pulse[index] != null)
		{
			StopCoroutine(_pulse[index]);
			_pulse[index] = null;

			if (_texts[index] != null)
				_texts[index].color = _baseColor[index];
		}
	}

	void SpawnFloater(int index, int value)
	{
		TMP_Text source = _texts[index];
		if (value == 0 || source == null) return;

		Transform parent = deltaLayer != null ? deltaLayer : transform;

		TMP_Text floater = Instantiate(source, parent);
		floater.gameObject.SetActive(true);
		floater.raycastTarget = false;
		floater.text = (value > 0 ? "+" : "") + value;
		floater.color = value > 0 ? gainColor : lossColor;
		floater.alignment = TextAlignmentOptions.Center;
		floater.fontStyle = FontStyles.Bold;
		floater.textWrappingMode = TextWrappingModes.NoWrap;

		RectTransform rt = floater.rectTransform;
		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.sizeDelta = new Vector2(220f, 64f);

		int j = _jitter[index]++ % 3;
		Vector3 offset = new Vector3((j - 1) * 14f, 8f, 0f);
		rt.position = source.rectTransform.position + offset;

		_floaters.Add(floater);
		StartCoroutine(FloaterRoutine(floater, rt));
	}

	IEnumerator FloaterRoutine(TMP_Text floater, RectTransform rt)
	{
		Vector3 start = rt.position;
		Color color = floater.color;
		float t = 0f;

		while (t < floaterDuration && floater != null)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / floaterDuration);

			rt.position = start + Vector3.up * (floaterRise * (1f - Mathf.Pow(1f - k, 3f)));

			Color c = color;
			c.a = 1f - k * k;
			floater.color = c;

			float pop = 1f + 0.25f * Mathf.Sin(Mathf.Clamp01(k * 3f) * Mathf.PI);
			rt.localScale = Vector3.one * pop;

			yield return null;
		}

		_floaters.Remove(floater);
		if (floater != null) Destroy(floater.gameObject);
	}

	IEnumerator PulseRoutine(int index)
	{
		TMP_Text text = _texts[index];

		while (text != null && _state != null)
		{
			float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI);
			text.color = Color.Lerp(_baseColor[index], dangerColor, k * 0.9f);
			yield return null;
		}

		_pulse[index] = null;
	}

	void Punch(int index, int value)
	{
		if (value == 0 || _texts[index] == null) return;

		if (_punch[index] != null)
			StopCoroutine(_punch[index]);

		_punch[index] = StartCoroutine(PunchRoutine(index));
	}

	IEnumerator PunchRoutine(int index)
	{
		TMP_Text text = _texts[index];
		Vector3 from = _baseScale[index];

		float t = 0f;
		const float duration = 0.18f;

		while (t < duration && text != null)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / duration);
			float s = 1f + 0.22f * Mathf.Sin(k * Mathf.PI);

			text.transform.localScale = from * s;
			yield return null;
		}

		if (text != null)
			text.transform.localScale = from;

		_punch[index] = null;
	}

	void ResetVisuals()
	{
		StopAllCoroutines();

		for (int i = 0; i < _texts.Length; i++)
		{
			_pulse[i] = null;
			_punch[i] = null;

			if (_texts[i] == null) continue;

			_texts[i].color = _baseColor[i];
			_texts[i].transform.localScale = _baseScale[i];
		}

		foreach (TMP_Text f in _floaters)
			if (f != null) Destroy(f.gameObject);

		_floaters.Clear();
	}

	static string BuildPending(RunState state)
	{
		if (state.pendingEffects == null || state.pendingEffects.Count == 0)
			return "";

		var parts = new List<string>();

		foreach (PendingEffect e in state.pendingEffects)
		{
			if (e == null || e.IsEmpty) continue;

			string when = e.turnsLeft == 1 ? "NEXT TURN" : $"IN {e.turnsLeft} TURNS";
			var stats = new List<string>();

			AddPendingStat(stats, e.survival, "SURV");
			AddPendingStat(stats, e.science, "SCI");
			AddPendingStat(stats, e.morale, "MOR");

			if (stats.Count > 0)
				parts.Add($"{when}: {string.Join(", ", stats)}");
		}

		if (parts.Count > 2)
		{
			int extra = parts.Count - 2;
			parts.RemoveRange(2, extra);
			parts.Add($"+{extra} MORE");
		}

		return string.Join("   ·   ", parts);
	}

	static void AddPendingStat(List<string> parts, int v, string label)
	{
		if (v == 0) return;
		parts.Add($"{(v > 0 ? "+" : "")}{v} {label}");
	}
}
