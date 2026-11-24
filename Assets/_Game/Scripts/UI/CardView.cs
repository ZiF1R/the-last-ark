using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] Image frame;
	[SerializeField] Image icon;
	[SerializeField] TMP_Text nameText;
	[SerializeField] TMP_Text roleText;
	[SerializeField] TMP_Text quoteText;
	[SerializeField] TMP_Text effectsText;
	[SerializeField] TMP_Text tagsText;
	[SerializeField] GameObject costBadge;
	[SerializeField] TMP_Text costText;
	[SerializeField] Button button;

	[Header("Tag Colors")]
	[SerializeField] Color tagColor = new Color(0.35f, 0.37f, 0.36f);
	[SerializeField] Color tagResonanceColor = new Color(0.72f, 0.52f, 0.16f);

	[Header("Juice")]
	[SerializeField] float hoverScale = 1.04f;
	[SerializeField] float hoverDuration = 0.09f;
	[SerializeField] float pickScale = 0.86f;
	[SerializeField] float enterScale = 0.92f;
	[SerializeField] float enterDuration = 0.16f;
	public CardDef Def { get; private set; }

	Action<CardDef> _onClick;
	RectTransform _rect;
	CanvasGroup _group;
	Vector3 _baseScale = Vector3.one;
	Coroutine _anim;
	bool _interactable = true;

	void Awake()
	{
		_rect = (RectTransform)transform;
		_baseScale = _rect.localScale;

		_group = GetComponent<CanvasGroup>();
		if (_group == null)
			_group = gameObject.AddComponent<CanvasGroup>();
	}

	public void Setup(CardDef card, Action<CardDef> onClick, RunState state = null)
	{
		Def = card;
		_onClick = onClick;

		nameText.text = card.displayName;
		roleText.text = card.role.ToString().ToUpperInvariant();
		quoteText.text = card.quote;
		effectsText.text = BuildEffects(card);

		if (tagsText != null)
		{
			string tags = BuildTags(card, state);
			tagsText.text = tags;
			tagsText.gameObject.SetActive(!string.IsNullOrEmpty(tags));
		}

		bool hasIcon = card.icon != null;
		icon.enabled = true;
		icon.sprite = card.icon;
		icon.color = hasIcon ? Color.white : RoleColor(card.role);
		icon.preserveAspect = true;

		bool costsTwo = card.slotCost > 1;
		costBadge.SetActive(costsTwo);
		if (costsTwo) costText.text = $"{card.slotCost} SLOTS";

		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => _onClick?.Invoke(card));

		StopAnim();
		_interactable = true;
		_group.alpha = 1f;
		_group.blocksRaycasts = true;
		_rect.localScale = _baseScale;
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_interactable) return;
		AnimateTo(hoverScale, hoverDuration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_interactable) return;
		AnimateTo(1f, hoverDuration);
	}

	public IEnumerator PlayEnter(float delay)
	{
		_group.alpha = 0f;
		_group.blocksRaycasts = false;
		_rect.localScale = _baseScale * enterScale;

		if (delay > 0f)
			yield return new WaitForSecondsRealtime(delay);

		float t = 0f;

		while (t < enterDuration && this != null)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / enterDuration));

			_group.alpha = k;
			_rect.localScale = Vector3.Lerp(_baseScale * enterScale, _baseScale, k);

			yield return null;
		}

		if (this == null) yield break;

		_group.alpha = 1f;
		_group.blocksRaycasts = true;
		_rect.localScale = _baseScale;
	}

	public IEnumerator PlayPick(float duration)
	{
		StopAnim();

		_interactable = false;
		_group.blocksRaycasts = false;

		Vector3 from = _rect.localScale;
		Vector3 to = _baseScale * pickScale;
		float t = 0f;

		while (t < duration && this != null)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / duration);

			_rect.localScale = Vector3.Lerp(from, to, k);
			_group.alpha = 1f - k;

			yield return null;
		}

		if (this == null) yield break;

		_group.alpha = 0f;
		_group.blocksRaycasts = false;
	}

	void AnimateTo(float multiplier, float duration)
	{
		if (this == null || !gameObject.activeInHierarchy) return;

		if (_anim != null)
			StopCoroutine(_anim);

		_anim = StartCoroutine(AnimateRoutine(multiplier, duration));
	}

	IEnumerator AnimateRoutine(float multiplier, float duration)
	{
		Vector3 from = _rect.localScale;
		Vector3 to = _baseScale * multiplier;
		float t = 0f;

		while (t < duration)
		{
			t += Time.unscaledDeltaTime;
			_rect.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(t / duration));
			yield return null;
		}

		_rect.localScale = to;
		_anim = null;
	}

	void StopAnim()
	{
		if (_anim == null) return;

		StopCoroutine(_anim);
		_anim = null;
	}

	string BuildTags(CardDef card, RunState state)
	{
		if (card.tags == null || card.tags.Length == 0) return "";

		var parts = new List<string>(card.tags.Length);

		foreach (CardTag tag in card.tags)
		{
			bool resonates = state != null && state.HasTag(tag);
			Color color = resonates ? tagResonanceColor : tagColor;

			parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{tag.ToString().ToUpperInvariant()}</color>");
		}

		return string.Join("  ·  ", parts);
	}

	static string BuildEffects(CardDef c)
	{
		var lines = new List<string>();

		AddLine(lines, c.survival, "SURVIVAL", "#6FA85C");
		AddLine(lines, c.science, "SCIENCE", "#4FA3A5");
		AddLine(lines, c.morale, "MORALE", "#E8A33D");

		string delayed = BuildDelayed(c.delayed);
		if (!string.IsNullOrEmpty(delayed))
			lines.Add(delayed);

		string period = c.passivePeriod > 1 ? $"EVERY {c.passivePeriod} TURNS" : "EACH TURN";
		AddPassive(lines, c.passiveSurvival, "SURVIVAL", "#6FA85C", period);
		AddPassive(lines, c.passiveScience, "SCIENCE", "#4FA3A5", period);
		AddPassive(lines, c.passiveMorale, "MORALE", "#E8A33D", period);

		return string.Join("\n", lines);
	}

	static void AddLine(List<string> lines, int v, string label, string hex)
	{
		if (v == 0) return;
		lines.Add($"<color={hex}>{(v > 0 ? "+" : "")}{v} {label}</color>");
	}

	static void AddPassive(List<string> lines, int v, string label, string hex, string period)
	{
		if (v == 0) return;
		lines.Add($"<size=100%><color={hex}>{(v > 0 ? "+" : "")}{v} {label} {period}</color></size>");
	}

	static string BuildDelayed(DelayedEffect d)
	{
		if (d == null || d.IsEmpty) return null;

		var parts = new List<string>();
		AddDelayedPart(parts, d.survival, "SURVIVAL", "#6FA85C");
		AddDelayedPart(parts, d.science, "SCIENCE", "#4FA3A5");
		AddDelayedPart(parts, d.morale, "MORALE", "#E8A33D");

		if (parts.Count == 0) return null;

		string when = d.turns == 1 ? "NEXT TURN" : $"IN {d.turns} TURNS";
		return $"<size=100%><color=#8A8F8C>{when}:</color> {string.Join(", ", parts)}</size>";
	}

	static void AddDelayedPart(List<string> parts, int v, string label, string hex)
	{
		if (v == 0) return;
		parts.Add($"<color={hex}>{(v > 0 ? "+" : "")}{v} {label}</color>");
	}

	static readonly Color[] RoleColors =
	{
		new Color(0.42f, 0.66f, 0.36f), // Biolog
        new Color(0.31f, 0.64f, 0.65f), // Engineer
        new Color(0.72f, 0.36f, 0.30f), // Soldier
        new Color(0.91f, 0.64f, 0.24f), // Medic
        new Color(0.85f, 0.75f, 0.55f), // Child
        new Color(0.45f, 0.45f, 0.50f), // Sick
        new Color(0.55f, 0.60f, 0.35f), // Farmer
        new Color(0.60f, 0.40f, 0.65f), // Mutant
    };

	static Color RoleColor(CardRole role) => RoleColors[(int)role % RoleColors.Length];
}
