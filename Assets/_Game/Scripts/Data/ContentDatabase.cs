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

	public CardDef GetCard(string id) => cards.Find(c => c.id == id);
	public EventDef GetEvent(string id) => events.Find(c => c.id == id);
	public EndingDef GetEnding(string id) => endings.Find(c => c.id == id);

	[ContextMenu("Validate")]
	public void Validate()
	{
		List<string> errors = new();
		List<string> warnings = new();

		ValidateCards(errors, warnings);
		ValidateEvents(errors, warnings);
		ValidateEndings(errors, warnings);
		ValidateFlags(errors, warnings);

		foreach (string w in warnings) Debug.LogWarning($"[Content] {w}", this);
		foreach (string e in errors) Debug.LogError($"[Content] {e}", this);

		string summary = $"cards={cards?.Count ?? 0}, events={events?.Count ?? 0}, endings={endings?.Count ?? 0}";

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

			bool doesNothing = c.survival == 0 && c.science == 0 && c.morale == 0
							   && string.IsNullOrWhiteSpace(c.eventOnPickId);
			if (doesNothing)
				warnings.Add($"{where}: карта ничего не делает (все дельты 0 и нет события).");

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

			if (e.choices == null || e.choices.Length != 2)
			{
				errors.Add($"{where}: нужно ровно 2 варианта, сейчас {(e.choices?.Length ?? 0)}.");
				continue;
			}

			for (int j = 0; j < e.choices.Length; j++)
			{
				Choice ch = e.choices[j];
				if (ch == null) { errors.Add($"{where}, Choice[{j}]: null."); continue; }

				if (string.IsNullOrWhiteSpace(ch.label))
					errors.Add($"{where}, Choice[{j}]: пустой label.");

				if (ch.survival == 0 && ch.science == 0 && ch.morale == 0 && string.IsNullOrWhiteSpace(ch.setFlag))
					warnings.Add($"{where}, Choice[{j}]: вариант ничего не меняет.");
			}

			Choice a = e.choices[0], b = e.choices[1];
			if (a != null && b != null
				&& a.label == b.label
				&& a.survival == b.survival
				&& a.science == b.science
				&& a.morale == b.morale)
			{
				warnings.Add($"{where}: оба варианта идентичны — выбор без выбора.");
			}
		}

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
	private void ValidateFlags(List<string> errors, List<string> warnings)
	{
		var setFlags = new HashSet<string>();

		if (events != null)
		{
			foreach (EventDef e in events)
			{
				if (e?.choices == null) continue;
				foreach (Choice ch in e.choices)
				{
					if (!string.IsNullOrWhiteSpace(ch?.setFlag))
						setFlags.Add(ch.setFlag);
				}

				if (e?.conditions == null) continue;
				foreach (EventCondition c in e.conditions)
				{
					if (c == null) continue;

					if (c.kind == ConditionKind.FlagSet && !setFlags.Contains(c.flag))
						warnings.Add($"Event '{e.id}': условие FlagSet '{c.flag}' нигде не выставляется — событие не сработает.");
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
				{
					if (!setFlags.Contains(f))
						warnings.Add($"Ending '{e.id}': requiredFlag '{f}' нигде не выставляется — концовка недостижима.");
				}
			}

			if (e.forbiddenFlags != null)
			{
				foreach (string f in e.forbiddenFlags)
				{
					if (!setFlags.Contains(f))
						warnings.Add($"Ending '{e.id}': forbiddenFlag '{f}' нигде не выставляется — условие всегда истинно.");
				}
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

		foreach (EventCondition c in e.conditions)
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
}
