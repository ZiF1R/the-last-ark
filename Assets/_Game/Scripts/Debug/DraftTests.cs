using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DraftTests
{
	const int OFFERS = 1000;
	const int WEIGHT_ROLLS = 20000;
	const float TOLERANCE = 3f;

	public static (int passed, int failed) Run(ContentDatabase db)
	{
		int passed = 0, failed = 0;

		void Check(string name, bool ok, string detail = "")
		{
			if (ok)
			{
				passed++;
				Debug.Log($"[Draft] OK   {name}");
			}
			else
			{
				failed++;
				string suffix = string.IsNullOrEmpty(detail) ? "" : $" — {detail}";
				Debug.LogError($"[Draft] FAIL {name}{suffix}");
			}
		}

		// --------- db check ----------
		if (db == null)
		{
			Debug.LogError("[Draft] ContentDatabase не назначена.");
			return (0, 1);
		}

		if (db.cards == null || db.cards.Count == 0)
		{
			Debug.LogError("[Draft] В базе нет карт — тестировать нечего.");
			return (0, 1);
		}

		if (db.cards.Count < 4)
		{
			Debug.LogWarning($"[Draft] В базе {db.cards.Count} карт. Для проверки дублей и фильтра " +
							 "нужно минимум 4 (лучше 6+), иначе тест может не дойти до взвешенного выбора.");
		}

		var draft = new DraftSystem();

		// ---------- duplicates ----------
		int dupOffers = 0;
		for (int i = 0; i < OFFERS; i++)
		{
			var offer = draft.BuildOffer(new RunState(), db, 3);
			if (offer.Count != offer.Distinct().Count()) dupOffers++;
		}
		Check("В оффере нет дублей", dupOffers == 0, $"офферов с дублями: {dupOffers}/{OFFERS}");

		// ---------- crew check ----------
		int hiredShown = 0;
		CardDef hired = db.cards[0];
		for (int i = 0; i < OFFERS; i++)
		{
			var st = new RunState();
			st.crew.Add(hired);
			if (draft.BuildOffer(st, db, 3).Contains(hired)) hiredShown++;
		}
		Check("Нанятая карта не попадает в оффер", hiredShown == 0, $"появлений: {hiredShown}/{OFFERS}");

		// ---------- offer size check ----------
		int expectedSize = Mathf.Min(3, db.cards.Count);
		int wrongSize = 0;
		for (int i = 0; i < OFFERS; i++)
		{
			if (draft.BuildOffer(new RunState(), db, 3).Count != expectedSize) wrongSize++;
		}
		Check($"Размер оффера = {expectedSize}", wrongSize == 0, $"неверных: {wrongSize}/{OFFERS}");

		// ---------- weights check ----------
		var freq = new Dictionary<CardDef, int>();
		for (int i = 0; i < WEIGHT_ROLLS; i++)
		{
			var one = draft.BuildOffer(new RunState(), db, 1);
			if (one.Count == 0) continue;
			freq.TryGetValue(one[0], out int n);
			freq[one[0]] = n + 1;
		}

		int totalWeight = db.cards.Sum(c => Mathf.Max(0, c.weight));
		Debug.Log($"[Draft] Суммарный вес {totalWeight}, прогонов {WEIGHT_ROLLS}");

		var deviations = new List<string>();
		var zeroWeightShown = new List<string>();

		foreach (var c in db.cards)
		{
			freq.TryGetValue(c, out int n);
			float actual = 100f * n / WEIGHT_ROLLS;
			float expected = totalWeight > 0
				? 100f * Mathf.Max(0, c.weight) / totalWeight
				: 100f / db.cards.Count;

			string line = $"[Draft] {c.id,-18} вес {c.weight,3}  " +
						  $"ожидалось {expected,5:F1}%  выпало {actual,5:F1}%";

			if (Mathf.Abs(actual - expected) > TOLERANCE)
			{
				deviations.Add($"{c.id}: {expected:F1}% → {actual:F1}%");
				Debug.LogWarning(line + "   <-- ОТКЛОНЕНИЕ");
			}
			else
			{
				Debug.Log(line);
			}

			if (c.weight == 0 && totalWeight > 0 && n > 0) zeroWeightShown.Add(c.id);
		}

		Check("Веса в пределах ±3 п.п.", deviations.Count == 0, string.Join("; ", deviations));
		Check("Карты с weight = 0 не выпадают", zeroWeightShown.Count == 0, string.Join(", ", zeroWeightShown));

		var small = new RunState();
		for (int i = 0; i < db.cards.Count - 1; i++) small.crew.Add(db.cards[i]);

		var lastOffer = draft.BuildOffer(small, db, 3);
		Check("Осталась 1 карта → оффер размером 1", lastOffer.Count == 1, $"размер: {lastOffer.Count}");

		var big = draft.BuildOffer(new RunState(), db, db.cards.Count);
		Check("BuildOffer возвращает свой список, а не db.cards", !ReferenceEquals(big, db.cards));

		return (passed, failed);
	}
}
