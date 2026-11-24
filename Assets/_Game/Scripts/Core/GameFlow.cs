using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour
{
	[Header("References")]
	public ContentDatabase db;
	public Image fader;

	[Header("Pacing (seconds, unscaled)")]
	[SerializeField] float faderDuration = 0.8f;
	[SerializeField] float pickSettle = 0.25f;
	[SerializeField] float synergySettle = 0.35f;
	[SerializeField] float turnSettle = 0.20f;
	[SerializeField] float eventSettle = 0.15f;
	[SerializeField] float epilogueSettle = 0.30f;

	enum Phase { Title, Draft, Resolve, Event, EventResult, Epilogue }

	RunState _state;
	List<CardDef> _offer;
	readonly DraftSystem _draftSystem = new();
	readonly EventPicker _eventSystem = new();
	Phase _phase;
	Image _faderImage;

	public RunState State => _state;

	void Start()
	{
		_phase = Phase.Title;
		_faderImage = fader.GetComponent<Image>();
		SetFaderAlpha(1f);

		StartRun();
	}

	[ContextMenu("Debug/Start Run")]
	void DebugStartRun() => StartRun();

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
		StopAllCoroutines();

		if (db == null || db.cards == null || db.cards.Count == 0)
		{
			Debug.LogError("[Start] ContentDatabase isn't assigned or empty");
			return;
		}

		_state = new();
		_phase = Phase.Title;
		GameEvents.RaiseRunStarted(_state);

		StartCoroutine(FadeTo(0f, faderDuration));
		StartCoroutine(EnterDraftRoutine());
	}

	private IEnumerator FadeTo(float target, float duration)
	{
		float start = 1f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			SetFaderAlpha(Mathf.Lerp(start, target, elapsed / duration));
			yield return null;
		}

		SetFaderAlpha(target);
	}

	public void SetFaderAlpha(float alpha)
	{
		Color color = _faderImage.color;
		color.a = Mathf.Clamp01(alpha);
		_faderImage.color = color;
	}

	IEnumerator EnterDraftRoutine()
	{
		if (_state.IsDead || _state.IsFull)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		_phase = Phase.Resolve;
		//_phase = Phase.Draft;

		_state.turn++;
		StatDelta tick = TurnTick.Apply(_state);
		GameEvents.RaiseStatsChanged(tick, _state);

		if (tick.HasAny)
			yield return new WaitForSecondsRealtime(turnSettle);

		if (_state.IsDead)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		_state.pendingEventId = null;
		_offer = _draftSystem.BuildOffer(_state, db);

		if (_offer.Count == 0)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		_phase = Phase.Draft;
		GameEvents.RaiseOfferReady(_offer);
	}

	public bool Pick(CardDef card)
	{
		if (_phase != Phase.Draft) return false;

		if (_offer == null || !_offer.Contains(card))
		{
			Debug.LogWarning($"[Pick] Offer is null or card with id={card.id} not in offer");
			return false;
		}

		_phase = Phase.Resolve;
		StartCoroutine(PickRoutine(card));

		return true;
	}

	IEnumerator PickRoutine(CardDef card)
	{
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

		yield return new WaitForSecondsRealtime(pickSettle);

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

			yield return new WaitForSecondsRealtime(synergySettle);
		}

		if (_state.IsDead)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		EventDef e = ResolveEvent();
		if (e != null)
		{
			ShowEvent(e);
			yield break;
		}

		if (_state.IsFull)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		yield return EnterDraftRoutine();
	}

	EventDef ResolveEvent()
	{
		if (string.IsNullOrEmpty(_state.pendingEventId))
			return _eventSystem.PickEvent(_state, db);

		EventDef e = db.GetEvent(_state.pendingEventId);

		if (e != null) return e;
			
		Debug.LogError($"[Event] Event with id={_state.pendingEventId} doesn't exist");
		return null;
	}

	void ShowEvent(EventDef e)
	{
		if (e == null)
		{
			Debug.LogError("[Event] Event is null");
			StartCoroutine(EnterDraftRoutine());
			return;
		}

		if (e.choices == null || e.choices.Length < 1 || e.choices.Length > 2)
		{
			Debug.LogError($"[Event] '{e.id}' has {e.choices?.Length ?? 0} choices, expected 1 or 2");
			StartCoroutine(EnterDraftRoutine());
			return;
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
			StartCoroutine(EnterDraftRoutine());
			return;
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

		_phase = Phase.Resolve;
		StartCoroutine(ContinueAfterEventRoutine());
	}

	IEnumerator ContinueAfterEventRoutine()
	{
		yield return new WaitForSecondsRealtime(eventSettle);

		if (_state.IsDead || _state.IsFull)
		{
			yield return EnterEpilogueRoutine();
			yield break;
		}

		yield return EnterDraftRoutine();
	}


	IEnumerator EnterEpilogueRoutine()
	{
		_phase = Phase.Epilogue;

		yield return new WaitForSecondsRealtime(epilogueSettle);

		EndingDef ending = EndingResolver.Resolve(_state, db);

		if (ending == null)
		{
			if (db.endings == null || db.endings.Count == 0)
			{
				Debug.LogError("[Epilogue] FAIL ending resolve: endings array is empty");
				yield break;
			}

			ending = db.endings[0];
			Debug.LogWarning("[Epilogue] Doesn't found any ending, using default");
		}

		GameEvents.RaiseRunEnded(ending, _state);
	}

	public void Restart() => StartRun();

}
