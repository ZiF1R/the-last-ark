using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
	[Header("References")]
	public ContentDatabase db;
	// views referenses

	enum Phase { Title, Draft, Resolve, Event, EventResult, Epilogue }

	RunState _state;
	List<CardDef> _offer;
	readonly DraftSystem _draftSystem = new();
	readonly EventPicker _eventSystem = new();
	Phase _phase;
	public RunState State => _state;

	void Start()
	{
		_phase = Phase.Title;
		StartRun();
	}

	[ContextMenu("Debug/Start Run")]
	void DebugStartRun()
	{
		StartRun();
	}

	[ContextMenu("Import Seed")]
	void ImportSeed()
	{
		db.cards = ContentSeed.Cards;
		db.events = ContentSeed.Events;
		db.endings = ContentSeed.Endings;
		db.synergies = ContentSeed.Synergies;
	}

	public void StartRun()
	{
		if (db == null || db.cards == null || db.cards.Count == 0)
		{
			Debug.LogError("[Start] ContentDatabase isn't assigned or empty");
			return;
		}

		_state = new();
		GameEvents.RaiseRunStarted(_state);

		EnterDraft();
	}

	void EnterDraft()
	{
		if (_state.IsDead || _state.IsFull)
		{
			EnterEpilogue(); return;
		}

		_phase = Phase.Draft;
		_state.turn++;

		StatDelta tick = TurnTick.Apply(_state);
		GameEvents.RaiseStatsChanged(tick, _state);

		if (_state.IsDead)
		{
			EnterEpilogue(); return;
		}

		_state.pendingEventId = null;
		_offer = _draftSystem.BuildOffer(_state, db);
		
		if (_offer.Count == 0)
		{
			EnterEpilogue(); return;
		}

		GameEvents.RaiseOfferReady(_offer);
	}

	public void Pick(CardDef card)
	{
		// prevent double click
		if (_phase != Phase.Draft) return;
		if (_offer == null || !_offer.Contains(card))
		{
			Debug.LogWarning($"[Pick] Offer is null or card with id={card.id} not in offer");
			return;
		}

		_phase = Phase.Resolve;

		StatDelta cardDelta = EffectResolver.ApplyCard(card, _state);
		_state.crew.Add(card);
		_state.leftBehind.Remove(card);
		_state.leftBehind.AddRange(
			_offer.FindAll(c =>
				c.id != card.id &&
				!_state.leftBehind.Contains(c)
			)
		);

		GameEvents.RaiseStatsChanged(cardDelta, _state);
		GameEvents.RaiseCrewChanged(_state);

		var synergies = SynergySystem.Evaluate(_state, db);

		if (synergies.Count > 0)
		{
			StatDelta synergyDelta = new(0, 0, 0);

			foreach (var (_, d) in synergies)
			{
				synergyDelta.survival += d.survival;
				synergyDelta.science += d.science;
				synergyDelta.morale += d.morale;
			}

			GameEvents.RaiseStatsChanged(synergyDelta, _state);

			foreach (var (rule, d) in synergies)
				GameEvents.RaiseSynergyFired(rule, d, _state);
		}

		if (_state.IsDead)
		{
			EnterEpilogue(); return;
		}

		EventDef e = ResolveEvent();
		if (e != null)
		{
			ShowEvent(e);
			return;
		}

		if (_state.IsFull)
		{
			EnterEpilogue(); return;
		}

		EnterDraft();
	}

	EventDef ResolveEvent()
	{
		if (string.IsNullOrEmpty(_state.pendingEventId))
		{
			return _eventSystem.PickEvent(_state, db);
		} else
		{
			EventDef e = db.GetEvent(_state.pendingEventId);

			if (e != null) return e;
			
			Debug.LogError($"[Event] Event with id={_state.pendingEventId} doesn't exist");
			return null;
		}
	}

	void ShowEvent(EventDef e)
	{
		if (e == null)
		{
			Debug.LogError("[Event] Event is null");
			EnterDraft(); return;
		}

		if (e.choices == null || e.choices.Length < 1 || e.choices.Length > 2)
		{
			Debug.LogError($"[Event] '{e.id}' has {e.choices?.Length ?? 0} choices, expected 1 or 2");
			EnterDraft(); return;
		}

		bool anyAvailable = false;
		foreach (Choice c in e.choices)
		{
			if (c != null && c.IsAvailable(_state))
			{
				anyAvailable = true; break;
			}
		}

		if (!anyAvailable)
		{
			Debug.LogError($"[Event] '{e.id}': all choices are locked — skipping");
			_state.usedEventIds.Add(e.id);
			EnterDraft(); return;
		}

		_phase = Phase.Event;
		_state.usedEventIds.Add(e.id);
		_state.pendingEventId = null;
		GameEvents.RaiseEventTriggered(e, _state);
	}

	public void ChooseEvent(Choice choice)
	{
		if (_phase != Phase.Event) return;
		if (choice == null) return;

		if (!choice.IsAvailable(_state))
		{
			Debug.LogWarning("[Event] Choice is locked");
			return;
		}

		_phase = Phase.EventResult;

		StatDelta delta = _state.ApplyStats(choice.survival, choice.science, choice.morale);

		if (!string.IsNullOrEmpty(choice.setFlag))
			_state.flags.Add(choice.setFlag);

		CardDef lost = null;

		if (choice.removal != null && choice.removal.IsActive)
		{
			lost = choice.removal.Apply(_state);

			if (lost == null)
				Debug.LogWarning($"[Event] '{choice.label}': removal target not found");
		}

		GameEvents.RaiseStatsChanged(delta, _state);

		if (lost != null)
			GameEvents.RaiseCrewChanged(_state);

		GameEvents.RaiseEventResolved(choice, lost);
	}

	public void ContinueAfterEvent()
	{
		if (_phase != Phase.EventResult) return;

		if (_state.IsDead || _state.IsFull)
		{
			EnterEpilogue(); return;
		}

		EnterDraft();
	}


	void EnterEpilogue()
	{
		_phase = Phase.Epilogue;
		EndingDef ending = EndingResolver.Resolve(_state, db);

		if (ending == null)
		{
			if (db.endings == null || db.endings.Count == 0)
			{
				Debug.LogError($"[Epilogue] FAIL ending resolve: endings array is empty");
				return;
			}

			ending = db.endings[0];
			Debug.LogWarning($"[Epilogue] Doesn't found any ending, using default");
		}

		GameEvents.RaiseRunEnded(ending, _state);
	}

	public void Restart() => StartRun();

}
