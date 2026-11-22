using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
	[Header("References")]
	public ContentDatabase db;
	// views referenses

	enum Phase { Title, Draft, Resolve, Event, EventResult, Epilogue }
	static readonly int[] DriftTurns = { 2, 4, 6 };

	RunState _state;
	List<CardDef> _offer;
	readonly DraftSystem _draftSystem = new();
	readonly EventPicker _eventSystem = new();
	Phase _phase;

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

		if (DriftTurns.Contains(_state.turn))
		{
			GameEvents.RaiseStatsChanged(
				_state.ApplyStats(-1, 0, 0),
				_state
			);
		}

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

		StatDelta delta = EffectResolver.ApplyCard(card, _state);
		_state.crew.Add(card);
		_state.leftBehind.Remove(card);
		_state.leftBehind.AddRange(
			_offer.FindAll(c =>
				c.id != card.id &&
				!_state.leftBehind.Contains(c)
			)
		);

		GameEvents.RaiseStatsChanged(delta, _state);
		GameEvents.RaiseCrewChanged(_state);

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
		if (e.choices.Length < 2)
		{
			Debug.LogError($"[ShowEvent] Event with id={e.id} doesn't have enough choices");
			EnterDraft();
			return;
		}

		_phase = Phase.Event;
		_state.usedEventIds.Add(e.id);
		_state.pendingEventId = null;
		GameEvents.RaiseEventTriggered(e);
	}

	public void ChooseEvent(Choice choice)
	{
		if (_phase != Phase.Event) return;

		_phase = Phase.EventResult;
		StatDelta delta = _state.ApplyStats(choice.survival, choice.science, choice.morale);

		if (!string.IsNullOrEmpty(choice.setFlag))
			_state.flags.Add(choice.setFlag);

		GameEvents.RaiseStatsChanged(delta, _state);
		GameEvents.RaiseEventResolved(choice);
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
