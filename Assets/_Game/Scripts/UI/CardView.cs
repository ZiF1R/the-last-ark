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
	[SerializeField] Image topBand;
	[SerializeField] TMP_Text nameText;
	[SerializeField] TMP_Text roleText;
	[SerializeField] TMP_Text quoteText;
	[SerializeField] TMP_Text effectsText;
	[SerializeField] TMP_Text tagsText;
	[SerializeField] GameObject costBadge;
	[SerializeField] TMP_Text costText;
	[SerializeField] Button button;

	[Header("Art")]
	// Swapped on hover: a non-integer scale would resample the 9-slice panel and
	// the pixel art would shimmer, so the frame changes sprite instead.
	[SerializeField] Sprite hoverFrame;
	[SerializeField] Sprite[] roleIcons;

	[Header("Juice")]
	// Hover never scales the card: a non-integer scale resamples the 9-slice and the
	// pixel art starts to shimmer. We lift the icon and brighten the frame instead.
	[SerializeField] float hoverLift = 6f;
	[SerializeField] float hoverDuration = 0.09f;
	[SerializeField] float pickScale = 0.86f;
	[SerializeField] float enterScale = 0.92f;
	[SerializeField] float enterDuration = 0.16f;
	public CardDef Def { get; private set; }

	Action<CardDef> _onClick;
	RectTransform _rect;
	CanvasGroup _group;
	Vector3 _baseScale = Vector3.one;
	Vector2 _iconBasePos;
	Color _frameBase = Color.white;
	Sprite _frameSprite;
	Coroutine _anim;
	bool _interactable = true;

	void Awake()
	{
		_rect = (RectTransform)transform;
		_baseScale = _rect.localScale;

		if (icon != null) _iconBasePos = icon.rectTransform.anchoredPosition;
		if (frame != null)
		{
			_frameBase = frame.color;
			_frameSprite = frame.sprite;
		}

		_group = GetComponent<CanvasGroup>();
		if (_group == null)
			_group = gameObject.AddComponent<CanvasGroup>();
	}

	public void Setup(CardDef card, Action<CardDef> onClick, RunState state = null)
	{
		Def = card;
		_onClick = onClick;

		nameText.text = card.displayName;
		roleText.text = UITheme.RoleLabel(card.role);
		quoteText.text = card.quote;
		effectsText.text = BuildEffects(card, state);

		if (tagsText != null)
		{
			string tags = BuildTags(card, state);
			tagsText.text = tags;
			tagsText.gameObject.SetActive(!string.IsNullOrEmpty(tags));
		}

		// Never overwrite the prefab sprite with a null reference: the role sigil lives
		// in the prefab and is only replaced when the card actually ships artwork.
		Sprite roleIcon = RoleIcon(card.role);

		if (card.icon != null)
		{
			icon.sprite = card.icon;
			icon.color = Color.white;
		}
		else if (roleIcon != null)
		{
			icon.sprite = roleIcon;
			icon.color = Color.white;
		}
		else
		{
			icon.color = UITheme.RoleColor(card.role);
		}

		icon.enabled = icon.sprite != null;
		icon.preserveAspect = true;

		if (topBand != null)
			topBand.color = UITheme.RoleColor(card.role);

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

		if (icon != null) icon.rectTransform.anchoredPosition = _iconBasePos;
		if (frame != null)
		{
			frame.color = _frameBase;
			frame.sprite = _frameSprite;
		}
	}

	Sprite RoleIcon(CardRole role)
	{
		if (roleIcons == null || roleIcons.Length == 0) return null;
		return roleIcons[(int)role % roleIcons.Length];
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_interactable) return;
		AnimateHover(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_interactable) return;
		AnimateHover(false);
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

	void AnimateHover(bool on)
	{
		if (this == null || !gameObject.activeInHierarchy) return;

		if (_anim != null)
			StopCoroutine(_anim);

		_anim = StartCoroutine(HoverRoutine(on));
	}

	IEnumerator HoverRoutine(bool on)
	{
		Vector2 fromPos = icon != null ? icon.rectTransform.anchoredPosition : Vector2.zero;
		Vector2 toPos = icon != null ? _iconBasePos + (on ? Vector2.up * hoverLift : Vector2.zero) : Vector2.zero;

		Color fromColor = frame != null ? frame.color : Color.white;
		Color toColor = on ? Color.Lerp(_frameBase, UITheme.Bone, 0.45f) : _frameBase;

		bool swap = frame != null && hoverFrame != null;
		if (swap) frame.sprite = on ? hoverFrame : _frameSprite;

		float t = 0f;

		while (t < hoverDuration)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / hoverDuration);

			if (icon != null) icon.rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, k);
			if (frame != null && !swap) frame.color = Color.Lerp(fromColor, toColor, k);

			yield return null;
		}

		if (icon != null) icon.rectTransform.anchoredPosition = toPos;
		if (frame != null && !swap) frame.color = toColor;

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
			Color color = resonates ? UITheme.Mor : UITheme.Muted;

			parts.Add($"<color=#{UITheme.Html(color)}>{tag.ToString().ToUpperInvariant()}</color>");
		}

		return string.Join("  ·  ", parts);
	}

	static string BuildEffects(CardDef c, RunState state)
	{
		var lines = new List<string>();

		AddLine(lines, c.survival, "SURVIVAL", UITheme.Surv);
		AddLine(lines, c.science, "SCIENCE", UITheme.Sci);
		AddLine(lines, c.morale, "MORALE", UITheme.Mor);

		string delayed = BuildDelayed(c.delayed);
		if (!string.IsNullOrEmpty(delayed))
			lines.Add(delayed);

		string period = c.passivePeriod > 1 ? $"/{c.passivePeriod} TURNS" : "/TURN";
		AddPassive(lines, c.passiveSurvival, "SURVIVAL", UITheme.Surv, period);
		AddPassive(lines, c.passiveScience, "SCIENCE", UITheme.Sci, period);
		AddPassive(lines, c.passiveMorale, "MORALE", UITheme.Mor, period);

		if (state != null && WouldKill(state, c))
			lines.Add($"<color=#{UITheme.Html(UITheme.Danger)}>FATAL</color>");

		return string.Join("\n", lines);
	}

	/// <summary>True when taking this card right now drops a stat to zero.</summary>
	public static bool WouldKill(RunState state, CardDef c)
	{
		return Mathf.Clamp(state.survival + c.survival, 0, 10) <= 0
			|| Mathf.Clamp(state.science + c.science, 0, 10) <= 0
			|| Mathf.Clamp(state.morale + c.morale, 0, 10) <= 0;
	}

	static void AddLine(List<string> lines, int v, string label, Color color)
	{
		if (v == 0) return;
		lines.Add($"<color=#{UITheme.Html(color)}>{label} {(v > 0 ? "+" : "")}{v}</color>");
	}

	static void AddPassive(List<string> lines, int v, string label, Color color, string period)
	{
		if (v == 0) return;
		lines.Add($"<color=#{UITheme.Html(color)}>{label} {(v > 0 ? "+" : "")}{v} <size={UITheme.FsMicro}>{period}</size></color>");
	}

	static string BuildDelayed(DelayedEffect d)
	{
		if (d == null || d.IsEmpty) return null;

		var parts = new List<string>();
		AddDelayedPart(parts, d.survival, "SURVIVAL", UITheme.Surv);
		AddDelayedPart(parts, d.science, "SCIENCE", UITheme.Sci);
		AddDelayedPart(parts, d.morale, "MORALE", UITheme.Mor);

		if (parts.Count == 0) return null;

		string when = d.turns == 1 ? "NEXT TURN" : $"IN {d.turns} TURNS";
		return $"<size={UITheme.FsMicro}><color=#{UITheme.Html(UITheme.Muted)}>{when}</color></size>\n{string.Join("\n", parts)}";
	}

	static void AddDelayedPart(List<string> parts, int v, string label, Color color)
	{
		if (v == 0) return;
		parts.Add($"<color=#{UITheme.Html(color)}>{label} {(v > 0 ? "+" : "")}{v}</color>");
	}
}
