using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Прогоняет N ранов без UI и без сцены: только DraftSystem + EffectResolver + EndingResolver.
/// Нужен, чтобы увидеть распределение концовок и смертность до того, как это появится в игре.
/// </summary>
public static class RunSimulator
{
	const int OFFER_SIZE = 3;
	const int MAX_TURNS = 20;   // страховка от бесконечного цикла

	public static void Run(ContentDatabase db, int runs = 1000, int seed = 12345)
	{
		if (db == null) { Debug.LogError("[Sim] ContentDatabase не назначена."); return; }
		if (db.cards == null || db.cards.Count == 0) { Debug.LogError("[Sim] В базе нет карт."); return; }
		if (db.endings == null || db.endings.Count == 0) { Debug.LogError("[Sim] В базе нет концовок."); return; }

		var draft = new DraftSystem();

		Debug.Log($"<b>[Sim] {runs} ранов на политику, seed {seed}, оффер {OFFER_SIZE}</b>");

		Simulate(db, runs, seed, draft, PickRandom, "RANDOM — случайный игрок");
		Simulate(db, runs, seed, draft, (offer, st) => PickGreedy(offer, st, db), "GREEDY — аккуратный игрок");
	}

	// ------------------------------------------------------------------
	static void Simulate(ContentDatabase db, int runs, int seed,
						 DraftSystem draft,
						 System.Func<List<CardDef>, RunState, CardDef> pick,
						 string label)
	{
		Random.InitState(seed);   // одинаковый seed → сравнимы результаты до/после правки баланса

		var endingCount = new Dictionary<string, int>();
		var pickedCards = new HashSet<CardDef>();
		var deathCause = new Dictionary<string, int>();
		var synergyCount = new Dictionary<string, int>();

		int deaths = 0, noEnding = 0, slotBlocked = 0, poolEmpty = 0;
		int totalTurns = 0, sumS = 0, sumSc = 0, sumM = 0;
		int synergyFires = 0;

		for (int r = 0; r < runs; r++)
		{
			var st = new RunState();

			while (!st.IsDead && !st.IsFull && st.turn < MAX_TURNS)
			{
				st.turn++;
				TurnTick.Apply(st);
				if (st.IsDead) break;

				List<CardDef> offer = draft.BuildOffer(st, db, OFFER_SIZE);

				// DraftSystem уже фильтрует по слотам; оставляем как страховку.
				int offeredBeforeSlotFilter = offer.Count;
				offer.RemoveAll(c => c.slotCost > st.SlotsLeft);

				if (offer.Count == 0)
				{
					if (offeredBeforeSlotFilter == 0) poolEmpty++;
					else slotBlocked++;
					break;
				}

				CardDef chosen = pick(offer, st);

				foreach (CardDef other in offer)
					if (!ReferenceEquals(other, chosen)) st.leftBehind.Add(other);

				EffectResolver.ApplyCard(chosen, st);
				st.crew.Add(chosen);
				pickedCards.Add(chosen);

				var fired = SynergySystem.Evaluate(st, db);
				synergyFires += fired.Count;

				foreach (var (rule, _) in fired)
				{
					string key = string.IsNullOrWhiteSpace(rule.id) ? "(no id)" : rule.id;

					synergyCount.TryGetValue(key, out int n);
					synergyCount[key] = n + 1;
				}

				st.pendingEventId = null;   // события в симуляции не обрабатываются
			}

			totalTurns += st.turn;
			sumS += st.survival;
			sumSc += st.science;
			sumM += st.morale;

			if (st.IsDead)
			{
				deaths++;
				string cause = st.survival <= 0 ? "Выживание"
							 : st.science <= 0 ? "Наука"
							 : "Мораль";
				deathCause.TryGetValue(cause, out int n);
				deathCause[cause] = n + 1;
			}

			EndingDef ending = EndingResolver.Resolve(st, db);
			if (ending == null) { noEnding++; continue; }

			string key2 = IdOf(ending);
			endingCount.TryGetValue(key2, out int count);
			endingCount[key2] = count + 1;
		}

		Report(db, runs, label, endingCount, pickedCards, deathCause,
			   deaths, noEnding, slotBlocked, poolEmpty, totalTurns, sumS, sumSc, sumM,
			   synergyFires, synergyCount);
	}

	// ------------------------------------------------------------------
	static void Report(ContentDatabase db, int runs, string label,
					   Dictionary<string, int> endingCount, HashSet<CardDef> pickedCards,
					   Dictionary<string, int> deathCause,
					   int deaths, int noEnding, int slotBlocked, int poolEmpty,
					   int totalTurns, int sumS, int sumSc, int sumM,
					   int synergyFires, Dictionary<string, int> synergyCount)
	{
		Debug.Log($"<b>[Sim] {label}</b>");

		string causes = deaths > 0
			? $" ({string.Join(", ", deathCause.Select(kv => $"{kv.Key} {kv.Value}"))})"
			: "";

		Debug.Log($"[Sim]   средняя длина {totalTurns / (float)runs:F1} ходов, " +
				  $"смертей {100f * deaths / runs:F1}%{causes}");

		Debug.Log($"[Sim]   средние статы: Выж {sumS / (float)runs:F1}  " +
				  $"Наука {sumSc / (float)runs:F1}  Мор {sumM / (float)runs:F1}");

		Debug.Log($"[Sim]   синергий за ран: {synergyFires / (float)runs:F2} (всего {synergyFires})");

		if (noEnding > 0)
			Debug.LogError($"[Sim]   {noEnding} ранов ({100f * noEnding / runs:F1}%) без концовки — " +
						   "в EndingResolver нет fallback.");

		if (poolEmpty > 0)
			Debug.LogError($"[Sim]   {poolEmpty} ранов оборвались: карты закончились, а слоты ещё есть.");

		if (slotBlocked > 0)
			Debug.LogError($"[Sim]   {slotBlocked} ранов застряли: остались слоты, " +
						   "но все оставшиеся карты занимают больше мест.");

		foreach (var kv in endingCount.OrderByDescending(kv => kv.Value))
			Debug.Log($"[Sim]   {kv.Key,-26} {kv.Value,5}   {100f * kv.Value / runs,5:F1}%");

		if (db.synergies != null && db.synergies.Count > 0)
		{
			foreach (var kv in synergyCount.OrderByDescending(kv => kv.Value))
				Debug.Log($"[Sim]   synergy {kv.Key,-28} {kv.Value,5}   {100f * kv.Value / runs,5:F1}% ранов");

			var neverFired = db.synergies
				.Where(s => s != null && !string.IsNullOrWhiteSpace(s.id) && !synergyCount.ContainsKey(s.id))
				.Select(s => s.id)
				.ToList();

			if (neverFired.Count > 0)
				Debug.LogWarning($"[Sim]   ни разу не сработали синергии: {string.Join(", ", neverFired)}");
		}

		var neverPicked = db.cards.Where(c => c != null && !pickedCards.Contains(c)).Select(c => c.id).ToList();
		if (neverPicked.Count > 0)
			Debug.LogWarning($"[Sim]   ни разу не выбраны: {string.Join(", ", neverPicked)}");

		var reached = new HashSet<string>(endingCount.Keys);
		var missed = db.endings
			.Where(e => e != null && !reached.Contains(IdOf(e)))
			.Select(e => e.id)
			.ToList();

		if (missed.Count > 0)
			Debug.LogWarning($"[Sim]   недостигнутые концовки: {string.Join(", ", missed)}");

		if (endingCount.Count > 0)
		{
			var top = endingCount.OrderByDescending(kv => kv.Value).First();

			if (top.Value > runs * 0.6f)
				Debug.LogWarning($"[Sim]   «{top.Key}» занимает {100f * top.Value / runs:F0}% — " +
								 "остальные концовки почти не видны.");
		}

		if (deaths > runs * 0.4f)
			Debug.LogWarning("[Sim]   смертность выше 40% — слишком жёстко.");

		if (deaths < runs * 0.02f)
			Debug.LogWarning("[Sim]   смертность ниже 2% — ставок нет, выбор не ощущается.");
	}

	// ------------------------------------------------------------------
	// Политики выбора. Обе смотрят только на оффер и текущее состояние.
	static CardDef PickRandom(List<CardDef> offer, RunState st)
		=> offer[Random.Range(0, offer.Count)];

	static CardDef PickGreedy(List<CardDef> offer, RunState st, ContentDatabase db)
	{
		CardDef best = offer[0];
		int bestScore = int.MinValue;

		foreach (CardDef c in offer)
		{
			int ds = c.delayed != null ? c.delayed.survival : 0;
			int dsc = c.delayed != null ? c.delayed.science : 0;
			int dm = c.delayed != null ? c.delayed.morale : 0;

			int ps = c.survival + ds;
			int psc = c.science + dsc;
			int pm = c.morale + dm;

			// Greedy учитывает синергии, которые карта завершит.
			// Это приближение: все дельты складываются до клампа.
			foreach (SynergyDef rule in SynergySystem.Preview(st, db, c))
			{
				ps += rule.survival;
				psc += rule.science;
				pm += rule.morale;
			}

			int s = Mathf.Clamp(st.survival + ps, 0, 10);
			int sc = Mathf.Clamp(st.science + psc, 0, 10);
			int m = Mathf.Clamp(st.morale + pm, 0, 10);

			// Сначала «не умереть» (минимальная стата), потом суммарная полезность.
			int lowest = Mathf.Min(s, Mathf.Min(sc, m));
			int score = lowest * 1000 + s + sc + m;

			if (score > bestScore) { bestScore = score; best = c; }
		}

		return best;
	}

	static string IdOf(EndingDef e)
		=> string.IsNullOrEmpty(e.id) ? (string.IsNullOrEmpty(e.title) ? "(без id)" : e.title) : e.id;
}
