using System;
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
		var sb = new StringBuilder();
		Append(sb, c.survival, "SURVIVAL", "#6FA85C");
		Append(sb, c.science, "SCIENCE", "#4FA3A5");
		Append(sb, c.morale, "MORALE", "#E8A33D");
		return sb.ToString().TrimEnd('\n');
	}

	static void Append(StringBuilder sb, int value, string label, string hex)
	{
		if (value == 0) return;
		string sign = value > 0 ? "+" : "";
		sb.Append($"<color={hex}>{sign}{value} {label}</color>\n");
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
