using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftView : MonoBehaviour
{
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;
	[SerializeField] RectTransform cardRow;
	[SerializeField] CardView cardPrefab;

	[Header("Juice")]
	[SerializeField] float entranceStagger = 0.055f;
	[SerializeField] float pickDuration = 0.14f;

	bool _locked;

	void OnEnable()
	{
		GameEvents.OfferReady += Show;
		GameEvents.RunStarted += OnRunStarted;
	}

	void OnDisable()
	{
		GameEvents.OfferReady -= Show;
		GameEvents.RunStarted -= OnRunStarted;
	}

	void OnRunStarted(RunState _)
	{
		StopAllCoroutines();
		_locked = false;
		panel.SetActive(false);
	}

	void Show(List<CardDef> offer)
	{
		StopAllCoroutines();
		_locked = false;

		Clear();
		panel.SetActive(true);

		for (int i = 0; i < offer.Count; i++)
		{
			CardView view = Instantiate(cardPrefab, cardRow);
			view.Setup(offer[i], OnCardClicked);
			StartCoroutine(view.PlayEnter(i * entranceStagger));
		}
	}

	void OnCardClicked(CardDef card)
	{
		if (_locked) return;

		CardView chosen = FindCard(card);
		if (!flow.Pick(card)) return;

		_locked = true;
		StartCoroutine(PickRoutine(chosen));
	}

	IEnumerator PickRoutine(CardView chosen)
	{
		if (chosen != null)
			yield return chosen.PlayPick(pickDuration);
		else
			yield return new WaitForSecondsRealtime(pickDuration);

		panel.SetActive(false);
	}

	CardView FindCard(CardDef card)
	{
		foreach (Transform child in cardRow)
		{
			CardView view = child.GetComponent<CardView>();
			if (view != null && view.Def == card) return view;
		}

		return null;
	}

	void Clear()
	{
		for (int i = cardRow.childCount - 1; i >= 0; i--)
		{
			Transform child = cardRow.GetChild(i);
			child.gameObject.SetActive(false);
			child.SetParent(null, false);
			Destroy(child.gameObject);
		}
	}
}
