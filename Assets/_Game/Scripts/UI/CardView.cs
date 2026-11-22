using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
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
	[SerializeField] GameObject costBadge;
	[SerializeField] TMP_Text costText;
	[SerializeField] Button button;

	Action<CardDef> _onClick;

	public void Setup(CardDef card, Action<CardDef> onClick)
	{
		_onClick = onClick;

		nameText.text = card.displayName;
		roleText.text = card.role.ToString().ToUpperInvariant();
		quoteText.text = card.quote;
		effectsText.text = BuildEffects(card);

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
