using System.Collections.Generic;
using UnityEngine;

public class DraftView : MonoBehaviour
{
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;
	[SerializeField] RectTransform cardRow;
	[SerializeField] CardView cardPrefab;

	void OnEnable() => GameEvents.OfferReady += Show;
	void OnDisable() => GameEvents.OfferReady -= Show;

	void Show(List<CardDef> offer)
	{
		Clear();
		panel.SetActive(true);

		foreach (CardDef card in offer)
		{
			CardView view = Instantiate(cardPrefab, cardRow);
			view.Setup(card, OnCardClicked);
		}
	}

	void OnCardClicked(CardDef card)
	{
		panel.SetActive(false);
		flow.Pick(card);
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
