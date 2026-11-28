using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// One-shot content validation for the seed database: draft tests, a 2000-run
/// simulation for both strategies, and reachability tables for endings, cards,
/// events and synergies. Writes Logs/content_report.txt.
///   Unity.exe -batchmode -quit -projectPath . -executeMethod ContentCheck.Run
/// </summary>
public static class ContentCheck
{
	public static void Run()
	{
		var db = UnityEngine.Object.FindAnyObjectByType<GameFlow>() != null
			? UnityEngine.Object.FindAnyObjectByType<GameFlow>().db
			: null;

		if (db == null)
		{
			// batch mode: load the asset directly
			db = UnityEditor.AssetDatabase.LoadAssetAtPath<ContentDatabase>(
				"Assets/_Game/Content/ContentDatabase.asset");
		}

		var log = new StringBuilder();

		log.AppendLine($"counts: cards={db.cards.Count} events={db.events.Count} " +
			$"endings={db.endings.Count} synergies={db.synergies.Count}");

		foreach (EndingDef e in db.endings)
			log.AppendLine($"  db ending {e.id,-24} prio {e.priority,3} " +
				$"win S[{e.minSurvival}..{e.maxSurvival}] Sc[{e.minScience}..{e.maxScience}] M[{e.minMorale}..{e.maxMorale}] " +
				$"flags[{string.Join(",", e.requiredFlags ?? new string[0])}]");

		var (passed, failed) = DraftTests.Run(db);
		log.AppendLine($"draft tests: pass {passed} / fail {failed}");

		RunSim(db, 2000, log);

		string path = Path.Combine(Application.dataPath, "../Logs/content_report.txt");
		File.WriteAllText(path, log.ToString());
		Debug.Log("[ContentCheck] wrote " + path);
	}

	static void RunSim(ContentDatabase db, int runs, StringBuilder log)
	{
		var draft = new DraftSystem();
		Report(db, runs, log, draft, PickRandom, "RANDOM");
		Report(db, runs, log, draft, PickGreedy, "GREEDY");
	}

	static CardDef PickRandom(List<CardDef> offer, RunState state, ContentDatabase db)
		=> offer[Random.Range(0, offer.Count)];

	static CardDef PickGreedy(List<CardDef> offer, RunState state, ContentDatabase db)
	{
		CardDef best = null;
		int bestScore = int.MinValue;

		foreach (CardDef c in offer)
		{
			int score = c.survival + c.science + c.morale;

			// Do not draft a card that kills on the spot.
			if (CardView.WouldKill(state, c))
				score -= 100;

			if (score > bestScore)
			{
				bestScore = score;
				best = c;
			}
		}

		return best != null ? best : offer[0];
	}

	static void Report(ContentDatabase db, int runs, StringBuilder log, DraftSystem draft,
		System.Func<List<CardDef>, RunState, ContentDatabase, CardDef> pick, string label)
	{
		var endings = new Dictionary<string, int>();
		var cards = new Dictionary<string, int>();
		var events = new Dictionary<string, int>();
		var synergies = new Dictionary<string, int>();

		int deaths = 0, filled = 0, poolEmpty = 0;
		int sumTurns = 0, sumS = 0, sumSc = 0, sumM = 0;
		int synergyFires = 0;
		int mismatches = 0;
		var finalStats = new Dictionary<string, int>();

		for (int r = 0; r < runs; r++)
		{
			var st = new RunState();
			int simTurn = 0;

			while (!st.IsDead && !st.IsFull && simTurn < 14)
			{
				simTurn++;
				st.turn++;
				TurnTick.Apply(st);
				if (st.IsDead) break;

				List<CardDef> offer = draft.BuildOffer(st, db, 3);
				offer.RemoveAll(c => c.slotCost > st.SlotsLeft);

				if (offer.Count == 0)
				{
					poolEmpty++;
					break;
				}

				CardDef chosen = pick(offer, st, db);
				foreach (CardDef c in offer)
					if (c.id == chosen.id) cards[c.id] = cards.GetValueOrDefault(c.id) + 1;

				EffectResolver.ApplyCard(chosen, st);
				st.crew.Add(chosen);
				st.leftBehind.Remove(chosen);

				SynergySystem.Evaluate(st, db);

				// on-pick events are forced, like GameFlow.ResolveEvent does
				EventDef ev;
				if (!string.IsNullOrEmpty(st.pendingEventId))
				{
					ev = db.GetEvent(st.pendingEventId);
					st.pendingEventId = null;
				}
				else
				{
					ev = PickEvent(st, db);
				}

				if (ev != null)
				{
					events[ev.id] = events.GetValueOrDefault(ev.id) + 1;
					st.usedEventIds.Add(ev.id);

					Choice choice = null;
					foreach (Choice c in ev.choices)
						if (c != null && c.IsAvailable(st)) { choice = c; break; }

					if (choice != null)
					{
						st.ApplyStats(choice.survival, choice.science, choice.morale);
						if (!string.IsNullOrEmpty(choice.setFlag)) st.flags.Add(choice.setFlag);
						if (choice.removal != null && choice.removal.IsActive) choice.removal.Apply(st);
					}
				}

				if (st.IsDead) break;
			}

			sumTurns += simTurn;
			sumS += st.survival;
			sumSc += st.science;
			sumM += st.morale;

			EndingDef e = EndingResolver.Resolve(st, db);
			string eid = e != null ? e.id : "NONE";
			endings[eid] = endings.GetValueOrDefault(eid) + 1;

			if (st.IsDead)
			{
				deaths++;

				// a dead run must resolve to a priority-100 (death) ending
				if (e == null || e.priority < 100)
				{
					mismatches++;
					if (mismatches <= 8)
						log.AppendLine($"  !! death resolved to {eid} with stats {st.survival}/{st.science}/{st.morale} " +
							$"crew {st.crew.Count} flags [{string.Join(",", st.flags)}]");
				}
			}
			else if (st.IsFull) filled++;
			else poolEmpty++;

			string key = $"{st.survival}/{st.science}/{st.morale}";
			finalStats[key] = finalStats.GetValueOrDefault(key) + 1;

			synergyFires += st.firedSynergies.Count;
			foreach (string s in st.firedSynergies)
				synergies[s] = synergies.GetValueOrDefault(s) + 1;
		}

		log.AppendLine($"\n=== {label} ({runs} runs) ===");
		log.AppendLine($"deaths {deaths * 100 / runs}%  filled {filled * 100 / runs}%  poolEmpty {poolEmpty * 100 / runs}%");
		log.AppendLine($"death-ending mismatches: {mismatches}");
		log.AppendLine($"avg turns {sumTurns / (float)runs:0.0}  avg stats {sumS / (float)runs:0.0}/{sumSc / (float)runs:0.0}/{sumM / (float)runs:0.0}");
		log.AppendLine($"synergies fired total {synergyFires} ({synergyFires / (float)runs:0.00}/run)");

		log.AppendLine("-- endings");
		foreach (var kv in endings)
			log.AppendLine($"  {kv.Key,-24} {kv.Value * 100f / runs:0.0}%");

		log.AppendLine("-- cards (times picked)");
		foreach (var kv in cards)
			log.AppendLine($"  {kv.Key,-24} {kv.Value}");

		log.AppendLine("-- events (times triggered)");
		foreach (var kv in events)
			log.AppendLine($"  {kv.Key,-24} {kv.Value * 100f / runs:0.0}%");

		log.AppendLine("-- synergies (times fired)");
		foreach (var kv in synergies)
			log.AppendLine($"  {kv.Key,-24} {kv.Value * 100f / runs:0.0}%");

		log.AppendLine("-- final stat lines (top 12)");
		foreach (var kv in FinalTop(finalStats, 12))
			log.AppendLine($"  {kv.Key,-10} {kv.Value * 100f / runs:0.0}%");
	}

	static List<KeyValuePair<string, int>> FinalTop(Dictionary<string, int> d, int n)
	{
		var list = new List<KeyValuePair<string, int>>(d);
		list.Sort((a, b) => b.Value.CompareTo(a.Value));
		return list.GetRange(0, Mathf.Min(n, list.Count));
	}

	// A minimal inline event picker: the real one lives in EventPicker and is
	// identical, but runs here without the UI layer.
	static EventDef PickEvent(RunState state, ContentDatabase db)
	{
		var candidates = new List<EventDef>();
		int bestPriority = int.MinValue;

		foreach (EventDef e in db.events)
		{
			if (e == null) continue;
			if (e.once && state.usedEventIds.Contains(e.id)) continue;
			if (!e.Matches(state)) continue;

			if (e.priority > bestPriority)
			{
				bestPriority = e.priority;
				candidates.Clear();
				candidates.Add(e);
			}
			else if (e.priority == bestPriority)
			{
				candidates.Add(e);
			}
		}

		return candidates.Count == 0 ? null : WeightedPicker.PickWeighted(candidates);
	}
}
