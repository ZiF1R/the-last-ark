using System;
using UnityEngine;

[Serializable]
public class SynergyDef
{
	[Tooltip("Уникальный id. По нему правило запоминается как «уже сработало».")]
	public string id;

	[Header("Condition")]
	public CardTag tagA;
	public CardTag tagB;

	[Tooltip("tagA == tagB: сколько всего нужно (код всё равно требует минимум 2). " +
			 "Иначе: сколько нужно членов с тегом B; тега A достаточно одного.")]
	[Min(1)] public int countB = 1;

	[Header("Effect — fires once per run")]
	public int survival, science, morale;

	[Tooltip("Необязательно. Флаг можно проверять в событиях и концовках.")]
	public string setFlag;

	[Header("Text")]
	[TextArea] public string text;

	public bool HasAnyEffect =>
		survival != 0 || science != 0 || morale != 0 || !string.IsNullOrWhiteSpace(setFlag);

	public bool IsNegative =>
		survival <= 0 && science <= 0 && morale <= 0 &&
		(survival < 0 || science < 0 || morale < 0);
}
