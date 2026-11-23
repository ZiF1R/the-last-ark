using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ContentDatabase", menuName = "Scriptable Objects/ContentDatabase")]
public class ContentDatabase : ScriptableObject
{
	public List<CardDef> cards = new();
	public List<EventDef> events = new();
	public List<EndingDef> endings = new();
	public List<SynergyDef> synergies = new();

	public CardDef GetCard(string id)
		=> cards?.Find(c => c != null && c.id == id);
	public EventDef GetEvent(string id)
		=> events?.Find(e => e != null && e.id == id);
	public EndingDef GetEnding(string id)
		=> endings?.Find(e => e != null && e.id == id);
	public SynergyDef GetSynergy(string id)
	=> synergies?.Find(s => s != null && s.id == id);


	[ContextMenu("Validate")]
	public void Validate()
	{
		List<string> errors = new();
		List<string> warnings = new();

		ValidateCards(errors, warnings);
		ValidateEvents(errors, warnings);
		ValidateEndings(errors, warnings);
		ValidateSynergies(errors, warnings);
		ValidateFlags(errors, warnings);

		foreach (string w in warnings) Debug.LogWarning($"[Content] {w}", this);
		foreach (string e in errors) Debug.LogError($"[Content] {e}", this);

		string summary = $"cards={cards?.Count ?? 0}, events={events?.Count ?? 0}, " +
						 $"endings={endings?.Count ?? 0}, synergies={synergies?.Count ?? 0}";

		if (errors.Count == 0 && warnings.Count == 0)
			Debug.Log($"[Content] OK ({summary})", this);
		else
			Debug.Log($"[Content] {errors.Count} ошибок, {warnings.Count} предупреждений ({summary})", this);
	}

	// ------------------------------------------------------------------
	private void ValidateCards(List<string> errors, List<string> warnings)
	{
		if (cards == null || cards.Count == 0)
		{
			errors.Add("cards пуст — драфтить нечего.");
			return;
		}

		for (int i = 0; i < cards.Count; i++)
		{
			CardDef c = cards[i];
			string where = $"Card[{i}]";

			if (c == null) { errors.Add($"{where}: null-элемент в списке."); continue; }

			if (string.IsNullOrWhiteSpace(c.id)) errors.Add($"{where} ({c.displayName}): пустой id.");
			else where = $"Card '{c.id}'";

			if (string.IsNullOrWhiteSpace(c.displayName)) errors.Add($"{where}: пустой displayName.");
			if (c.slotCost < 1) errors.Add($"{where}: slotCost = {c.slotCost}, должен быть >= 1.");
			if (c.weight < 0) errors.Add($"{where}: weight = {c.weight}, не может быть < 0.");

			if (c.tags == null || c.tags.Length == 0)
			{
				warnings.Add($"{where}: нет тегов — карта не участвует в событиях, синергиях и концовках.");
			}
			else if (System.Enum.TryParse(c.role.ToString(), out CardTag roleTag) && !c.tags.Contains(roleTag))
			{
				warnings.Add($"{where}: роль {c.role}, но тега {roleTag} нет в tags.");
			}

			if (c.icon == null)
				warnings.Add($"{where}: не назначена иконка.");

			bool hasDelayed = c.delayed != null && !c.delayed.IsEmpty;
			bool doesNothing = c.survival == 0 && c.science == 0 && c.morale == 0
				&& c.passiveSurvival == 0 && c.passiveScience == 0 && c.passiveMorale == 0
				&& !hasDelayed
				&& string.IsNullOrWhiteSpace(c.eventOnPickId);

			if (doesNothing)
				warnings.Add($"{where}: карта ничего не делает (все дельты 0 и нет события).");

			if (c.delayed != null && !c.delayed.IsEmpty)
			{
				if (c.delayed.turns < 1)
					errors.Add($"{where}: delayed.turns = {c.delayed.turns}, должен быть >= 1.");

				if (c.delayed.turns > 4)
					warnings.Add($"{where}: delayed.turns = {c.delayed.turns} — игрок может не дожить до срабатывания.");

				int delayedPower = Mathf.Abs(c.delayed.survival)
								 + Mathf.Abs(c.delayed.science)
								 + Mathf.Abs(c.delayed.morale);

				if (delayedPower > 3)
					warnings.Add($"{where}: отложенный эффект {delayedPower} пункта — проверь баланс.");
			}

			if (c.passivePeriod < 1)
				errors.Add($"{where}: passivePeriod = {c.passivePeriod}, должен быть >= 1.");

			int passivePower = Mathf.Abs(c.passiveSurvival) + Mathf.Abs(c.passiveScience) + Mathf.Abs(c.passiveMorale);
			if (passivePower > 2)
				warnings.Add($"{where}: пассив {passivePower} пункта за тик — проверь баланс.");

			if (!string.IsNullOrWhiteSpace(c.eventOnPickId) && GetEvent(c.eventOnPickId) == null)
				errors.Add($"{where}: eventOnPickId '{c.eventOnPickId}' не найден в events.");
		}

		foreach (var g in cards.Where(c => c != null).GroupBy(c => c.id).Where(g => g.Count() > 1))
			errors.Add($"Card: id '{g.Key}' встречается {g.Count()} раз.");
	}

	// ------------------------------------------------------------------
	private void ValidateEvents(List<string> errors, List<string> warnings)
	{
		if (events == null || events.Count == 0)
		{
			warnings.Add("events пуст — событий в игре не будет.");
			return;
		}

		for (int i = 0; i < events.Count; i++)
		{
			EventDef e = events[i];
			string where = $"Event[{i}]";

			if (e == null) { errors.Add($"{where}: null-элемент в списке."); continue; }

			if (string.IsNullOrWhiteSpace(e.id)) errors.Add($"{where}: пустой id.");
			else where = $"Event '{e.id}'";

			if (string.IsNullOrWhiteSpace(e.title)) errors.Add($"{where}: пустой title.");
			if (string.IsNullOrWhiteSpace(e.body)) warnings.Add($"{where}: пустой body.");

			if (e.minTurn > e.maxTurn)
				errors.Add($"{where}: minTurn ({e.minTurn}) > maxTurn ({e.maxTurn}) — событие не сработает никогда.");

			ValidateEventConditions(e, errors, warnings);

			if ((e.conditions == null || e.conditions.Length == 0)
				&& !e.once
				&& e.priority > 0)
			{
				warnings.Add($"Event '{e.id}': нет условий, once = false и priority > 0 — " +
							 "будет выпадать после каждого хода.");
			}

			if (e.choices == null || e.choices.Length < 1 || e.choices.Length > 2)
			{
				errors.Add($"{where}: нужно 1 или 2 варианта, сейчас {(e.choices?.Length ?? 0)}.");
				continue;
			}

			bool anyUnconditional = false;
			foreach (Choice c in e.choices)
				if (c != null && (c.requirements == null || c.requirements.Length == 0))
					anyUnconditional = true;

			if (!anyUnconditional)
				errors.Add($"{where}: все варианты условные — возможен софтлок.");

			for (int j = 0; j < e.choices.Length; j++)
			{
				Choice ch = e.choices[j];
				if (ch == null) { errors.Add($"{where}, Choice[{j}]: null."); continue; }

				if (ch.requirements != null)
					foreach (var r in ch.requirements)
						if (r == null) errors.Add($"{where}, Choice[{j}]: null-требование.");

				if (string.IsNullOrWhiteSpace(ch.label))
					errors.Add($"{where}, Choice[{j}]: пустой label.");

				bool changesStats = ch.survival != 0 || ch.science != 0 || ch.morale != 0;
				bool setsFlag = !string.IsNullOrWhiteSpace(ch.setFlag);
				bool removesCrew = ch.removal != null && ch.removal.IsActive;

				if (!changesStats && !setsFlag && !removesCrew)
					warnings.Add($"{where}, Choice[{j}]: вариант ничего не меняет.");

				if (removesCrew)
					ValidateRemoval(e, ch, j, where, errors, warnings);
			}

			if (e.choices.Length == 1)
			{
				Choice only = e.choices[0];

				if (only != null && string.IsNullOrWhiteSpace(only.result))
					warnings.Add($"{where}: событие без выбора, но result пуст — игрок не увидит последствий.");

				if (!e.once)
					warnings.Add($"{where}: событие без выбора с once = false будет повторяться каждый подходящий ход.");
			}

			if (e.choices.Length == 2)
			{
				Choice c1 = e.choices[0], c2 = e.choices[1];

				bool aRemoves = c1?.removal != null && c1.removal.IsActive;
				bool bRemoves = c2?.removal != null && c2.removal.IsActive;

				bool sameRemoval = aRemoves == bRemoves;
				if (sameRemoval && aRemoves)
					sameRemoval = c1.removal.selector == c2.removal.selector
							   && c1.removal.tag == c2.removal.tag
							   && c1.removal.role == c2.removal.role;

				if (c1 != null && c2 != null
					&& c1.label == c2.label
					&& c1.survival == c2.survival
					&& c1.science == c2.science
					&& c1.morale == c2.morale
					&& sameRemoval)
				{
					warnings.Add($"{where}: оба варианта идентичны — выбор без выбора.");
				}
			}
		}

		int beatCount = events.Count(x => x != null && x.choices != null && x.choices.Length == 1);
		if (beatCount > 2)
			warnings.Add($"Событий без выбора: {beatCount}. Рекомендуется 1–2 на ран.");

		foreach (var g in events.Where(e => e != null).GroupBy(e => e.id).Where(g => g.Count() > 1))
			errors.Add($"Event: id '{g.Key}' встречается {g.Count()} раз.");
	}

	// ------------------------------------------------------------------
	private void ValidateEndings(List<string> errors, List<string> warnings)
	{
		if (endings == null || endings.Count == 0)
		{
			errors.Add("endings пуст — игру нечем закончить.");
			return;
		}

		for (int i = 0; i < endings.Count; i++)
		{
			EndingDef e = endings[i];
			string where = $"Ending[{i}]";

			if (e == null) { errors.Add($"{where}: null-элемент в списке."); continue; }

			if (string.IsNullOrWhiteSpace(e.id)) errors.Add($"{where}: пустой id.");
			else where = $"Ending '{e.id}'";

			if (string.IsNullOrWhiteSpace(e.title)) errors.Add($"{where}: пустой title.");
			if (string.IsNullOrWhiteSpace(e.body)) warnings.Add($"{where}: пустой body.");

			if (e.minSurvival > e.maxSurvival) errors.Add($"{where}: minSurvival > maxSurvival.");
			if (e.minScience > e.maxScience) errors.Add($"{where}: minScience > maxScience.");
			if (e.minMorale > e.maxMorale) errors.Add($"{where}: minMorale > maxMorale.");

			if (e.requiredFlags != null && e.forbiddenFlags != null
				&& e.requiredFlags.Intersect(e.forbiddenFlags).Any())
			{
				errors.Add($"{where}: один флаг одновременно required и forbidden — концовка недостижима.");
			}
		}

		bool hasFallback = endings.Any(e => e != null
			&& (e.requiredFlags == null || e.requiredFlags.Length == 0)
			&& (e.forbiddenFlags == null || e.forbiddenFlags.Length == 0)
			&& e.minSurvival == 0 && e.minScience == 0 && e.minMorale == 0
			&& e.maxSurvival == 10 && e.maxScience == 10 && e.maxMorale == 10);

		if (!hasFallback)
			warnings.Add("Нет концовки-заглушки без условий (min=0, max=10, без флагов). Если ни одно условие не совпадёт, игрок увидит пустой экран.");

		foreach (var g in endings.Where(e => e != null).GroupBy(e => e.id).Where(g => g.Count() > 1))
			errors.Add($"Ending: id '{g.Key}' встречается {g.Count()} раз.");
	}

	// ------------------------------------------------------------------
	private void ValidateSynergies(List<string> errors, List<string> warnings)
	{
		if (synergies == null || synergies.Count == 0)
		{
			warnings.Add("synergies пуст — теги не дают бонусов, система синергий не участвует в игре.");
			return;
		}

		var ids = new HashSet<string>();
		var combos = new HashSet<string>();

		for (int i = 0; i < synergies.Count; i++)
		{
			SynergyDef s = synergies[i];
			string where = $"Synergy[{i}]";

			if (s == null) { errors.Add($"{where}: null-элемент в списке."); continue; }

			if (string.IsNullOrWhiteSpace(s.id))
			{
				errors.Add($"{where}: пустой id.");
			}
			else
			{
				where = $"Synergy '{s.id}'";

				if (!ids.Add(s.id))
					errors.Add($"{where}: id повторяется — вторая копия никогда не сработает.");
			}

			if (!System.Enum.IsDefined(typeof(CardTag), s.tagA))
				errors.Add($"{where}: tagA = {(int)s.tagA} — нет такого CardTag.");

			if (!System.Enum.IsDefined(typeof(CardTag), s.tagB))
				errors.Add($"{where}: tagB = {(int)s.tagB} — нет такого CardTag.");

			if (s.countB < 1)
				errors.Add($"{where}: countB = {s.countB}, должен быть >= 1.");

			if (s.tagA == s.tagB && s.countB < 2)
				warnings.Add($"{where}: tagA == tagB, но countB = {s.countB}. " +
							 "Код всё равно потребует 2 — поставь 2, чтобы данные не врали.");

			int needA = s.tagA == s.tagB ? Mathf.Max(2, s.countB) : 1;
			int needB = s.tagA == s.tagB ? 0 : Mathf.Max(1, s.countB);

			if (needA > RunState.MAX_SLOTS || needB > RunState.MAX_SLOTS)
				errors.Add($"{where}: нужно больше членов экипажа, чем слотов в ковчеге ({RunState.MAX_SLOTS}).");

			int cardsWithA = cards == null ? 0 : cards.Count(c =>
				c != null && c.tags != null && c.tags.Contains(s.tagA));

			int cardsWithB = s.tagA == s.tagB
				? cardsWithA
				: (cards == null ? 0 : cards.Count(c =>
					c != null && c.tags != null && c.tags.Contains(s.tagB)));

			if (cardsWithA < needA)
				warnings.Add($"{where}: тег {s.tagA} есть у {cardsWithA} карт(ы), " +
							 $"а нужно {needA} — правило недостижимо.");

			if (s.tagA != s.tagB && cardsWithB < needB)
				warnings.Add($"{where}: тег {s.tagB} есть у {cardsWithB} карт(ы), " +
							 $"а нужно {needB} — правило недостижимо.");

			if (s.tagA != s.tagB && cards != null && cards.Any(c =>
				c != null && c.tags != null &&
				c.tags.Contains(s.tagA) && c.tags.Contains(s.tagB)))
			{
				warnings.Add($"{where}: есть карта с обоими тегами ({s.tagA} + {s.tagB}) — " +
							 "правило может сработать от одного члена экипажа.");
			}

			if (!s.HasAnyEffect)
				warnings.Add($"{where}: правило ничего не делает (все дельты 0 и setFlag пуст).");

			int power = Mathf.Abs(s.survival) + Mathf.Abs(s.science) + Mathf.Abs(s.morale);
			if (power > 4)
				warnings.Add($"{where}: суммарная сила {power} — проверь баланс, " +
							 "синергия не должна решать ран одна.");

			if (string.IsNullOrWhiteSpace(s.text))
				warnings.Add($"{where}: пустой text — игрок увидит только заголовок и дельты.");

			if (!string.IsNullOrWhiteSpace(s.setFlag) && !IsFlagReferenced(s.setFlag))
				warnings.Add($"{where}: setFlag '{s.setFlag}' нигде не читается — флаг бесполезен.");

			int comboCount = s.tagA == s.tagB ? Mathf.Max(2, s.countB) : Mathf.Max(1, s.countB);
			string combo = $"{s.tagA}|{s.tagB}|{comboCount}";

			if (!combos.Add(combo))
				warnings.Add($"{where}: дубликат условия {s.tagA} + {s.tagB} × {comboCount} — " +
							 "сработает только первое правило.");
		}
	}

	// ------------------------------------------------------------------
	private void ValidateFlags(List<string> errors, List<string> warnings)
	{
		var setFlags = new HashSet<string>();

		// 1. Собираем ВСЕ флаги, которые где-либо выставляются.
		if (events != null)
		{
			foreach (EventDef e in events)
			{
				if (e?.choices == null) continue;

				foreach (Choice ch in e.choices)
					if (!string.IsNullOrWhiteSpace(ch?.setFlag))
						setFlags.Add(ch.setFlag);
			}
		}

		if (synergies != null)
		{
			foreach (SynergyDef s in synergies)
				if (!string.IsNullOrWhiteSpace(s?.setFlag))
					setFlags.Add(s.setFlag);
		}

		// 2. Только теперь проверяем все места, где флаг читается.
		if (events != null)
		{
			foreach (EventDef e in events)
			{
				if (e == null) continue;

				CheckConditions(e.conditions, $"Event '{e.id}'", setFlags, warnings);

				if (e.choices == null) continue;

				for (int i = 0; i < e.choices.Length; i++)
				{
					Choice ch = e.choices[i];
					if (ch?.requirements == null) continue;

					CheckConditions(ch.requirements, $"Event '{e.id}', Choice[{i}]", setFlags, warnings);
				}
			}
		}

		if (endings == null) return;

		foreach (EndingDef e in endings)
		{
			if (e == null) continue;

			if (e.requiredFlags != null)
			{
				foreach (string f in e.requiredFlags)
					if (!setFlags.Contains(f))
						warnings.Add($"Ending '{e.id}': requiredFlag '{f}' нигде не выставляется — концовка недостижима.");
			}

			if (e.forbiddenFlags != null)
			{
				foreach (string f in e.forbiddenFlags)
					if (!setFlags.Contains(f))
						warnings.Add($"Ending '{e.id}': forbiddenFlag '{f}' нигде не выставляется — условие всегда истинно.");
			}
		}
	}

	private bool IsFlagReferenced(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag)) return false;

		if (events != null)
		{
			foreach (EventDef e in events)
			{
				if (e == null) continue;

				if (e.conditions != null)
				{
					foreach (StateCondition c in e.conditions)
					{
						if (c == null) continue;

						if ((c.kind == ConditionKind.FlagSet || c.kind == ConditionKind.FlagAbsent)
							&& c.flag == flag)
							return true;
					}
				}

				if (e.choices == null) continue;

				foreach (Choice ch in e.choices)
				{
					if (ch?.requirements == null) continue;

					foreach (StateCondition c in ch.requirements)
					{
						if (c == null) continue;

						if ((c.kind == ConditionKind.FlagSet || c.kind == ConditionKind.FlagAbsent)
							&& c.flag == flag)
							return true;
					}
				}
			}
		}

		if (endings != null)
		{
			foreach (EndingDef e in endings)
			{
				if (e == null) continue;

				if (e.requiredFlags != null && e.requiredFlags.Contains(flag)) return true;
				if (e.forbiddenFlags != null && e.forbiddenFlags.Contains(flag)) return true;
			}
		}

		return false;
	}

	private static void CheckConditions(StateCondition[] conditions, string where,
									HashSet<string> setFlags, List<string> warnings)
	{
		if (conditions == null) return;

		foreach (StateCondition c in conditions)
		{
			if (c == null) continue;

			if (c.kind == ConditionKind.FlagSet
				&& !string.IsNullOrWhiteSpace(c.flag)
				&& !setFlags.Contains(c.flag))
			{
				warnings.Add($"{where}: условие FlagSet '{c.flag}' нигде не выставляется — " +
							 "условие никогда не выполнится.");
			}

			if (c.kind == ConditionKind.FlagAbsent
				&& !string.IsNullOrWhiteSpace(c.flag)
				&& !setFlags.Contains(c.flag))
			{
				warnings.Add($"{where}: условие FlagAbsent '{c.flag}' нигде не выставляется — " +
							 "условие всегда истинно.");
			}
		}
	}

	private void ValidateEventConditions(EventDef e, List<string> errors, List<string> warnings)
	{
		if (e.conditions == null || e.conditions.Length == 0)
		{
			warnings.Add($"Event '{e.id}': нет условий — событие может сработать в любой момент.");
			return;
		}

		var tagsPresent = new HashSet<CardTag>();
		var tagsAbsent = new HashSet<CardTag>();
		var rolesPresent = new HashSet<CardRole>();
		var rolesAbsent = new HashSet<CardRole>();
		var flagsSet = new HashSet<string>();
		var flagsAbsent = new HashSet<string>();
		var statMin = new Dictionary<StatKind, int>();
		var statMax = new Dictionary<StatKind, int>();

		foreach (StateCondition c in e.conditions)
		{
			if (c == null) { errors.Add($"Event '{e.id}': null-условие."); continue; }

			switch (c.kind)
			{
				case ConditionKind.TagPresent: tagsPresent.Add(c.tag); break;
				case ConditionKind.TagAbsent: tagsAbsent.Add(c.tag); break;
				case ConditionKind.RolePresent: rolesPresent.Add(c.role); break;
				case ConditionKind.RoleAbsent: rolesAbsent.Add(c.role); break;

				case ConditionKind.FlagSet:
					if (string.IsNullOrWhiteSpace(c.flag)) errors.Add($"Event '{e.id}': FlagSet с пустым flag.");
					else flagsSet.Add(c.flag);
					break;

				case ConditionKind.FlagAbsent:
					if (string.IsNullOrWhiteSpace(c.flag)) errors.Add($"Event '{e.id}': FlagAbsent с пустым flag.");
					else flagsAbsent.Add(c.flag);
					break;

				case ConditionKind.StatAtLeast:
					if (c.value < 0 || c.value > 10)
						warnings.Add($"Event '{e.id}': StatAtLeast {c.stat} = {c.value}, вне диапазона 0..10.");
					statMin[c.stat] = c.value;
					break;

				case ConditionKind.StatAtMost:
					if (c.value < 0 || c.value > 10)
						warnings.Add($"Event '{e.id}': StatAtMost {c.stat} = {c.value}, вне диапазона 0..10.");
					statMax[c.stat] = c.value;
					break;

				case ConditionKind.CrewCountAtLeast:
				case ConditionKind.CrewCountAtMost:
					if (c.value < 0) errors.Add($"Event '{e.id}': {c.kind} с отрицательным value.");
					break;
			}
		}

		foreach (var t in tagsPresent.Intersect(tagsAbsent))
			errors.Add($"Event '{e.id}': тег {t} одновременно требуется и запрещён — событие невозможно.");
		foreach (var r in rolesPresent.Intersect(rolesAbsent))
			errors.Add($"Event '{e.id}': роль {r} одновременно требуется и запрещена.");
		foreach (var f in flagsSet.Intersect(flagsAbsent))
			errors.Add($"Event '{e.id}': флаг '{f}' одновременно требуется и запрещён.");
		foreach (var kv in statMin)
			if (statMax.TryGetValue(kv.Key, out int max) && kv.Value > max)
				errors.Add($"Event '{e.id}': {kv.Key} >= {kv.Value} и <= {max} — событие невозможно.");

		if (e.weight <= 0 && e.priority <= 0)
			warnings.Add($"Event '{e.id}': weight <= 0 и priority = 0 — событие не будет выбрано никогда.");
	}

	private void ValidateRemoval(EventDef e, Choice ch, int index, string where,
							 List<string> errors, List<string> warnings)
	{
		CrewRemoval r = ch.removal;
		if (r == null || !r.IsActive) return;

		string at = $"{where}, Choice[{index}]";

		if (r.selector == CrewSelector.ById)
		{
			bool exists = cards != null && cards.Any(c => c != null && c.id == r.cardId);

			if (!exists)
				errors.Add($"{at}: removal по id '{r.cardId}', но такой карты нет в базе.");
		} else if (r.selector == CrewSelector.ByTag)
		{
			bool tagExists = cards != null && cards.Any(c => c != null
				&& c.tags != null && c.tags.Contains(r.tag));

			if (!tagExists)
				errors.Add($"{at}: removal по тегу {r.tag}, но ни у одной карты нет этого тега.");

			bool guaranteed = ConditionsMentionTag(e.conditions, r.tag)
						   || ConditionsMentionTag(ch.requirements, r.tag);

			if (!guaranteed)
				warnings.Add($"{at}: removal по тегу {r.tag}, но ни условие события, ни требование варианта не гарантируют наличие такого члена экипажа.");
		}
		else if (r.selector == CrewSelector.ByRole)
		{
			bool roleExists = cards != null && cards.Any(c => c != null && c.role == r.role);

			if (!roleExists)
				errors.Add($"{at}: removal по роли {r.role}, но ни у одной карты нет этой роли.");

			bool guaranteed = ConditionsMentionRole(e.conditions, r.role)
						   || ConditionsMentionRole(ch.requirements, r.role);

			if (!guaranteed)
				warnings.Add($"{at}: removal по роли {r.role}, но ни условие события, ни требование варианта не гарантируют наличие такого члена экипажа.");
		}
	}

	private static bool ConditionsMentionTag(StateCondition[] conditions, CardTag tag)
	{
		if (conditions == null) return false;

		foreach (StateCondition c in conditions)
			if (c != null && c.kind == ConditionKind.TagPresent && c.tag == tag)
				return true;

		return false;
	}

	private static bool ConditionsMentionRole(StateCondition[] conditions, CardRole role)
	{
		if (conditions == null) return false;

		foreach (StateCondition c in conditions)
			if (c != null && c.kind == ConditionKind.RolePresent && c.role == role)
				return true;

		return false;
	}
}
