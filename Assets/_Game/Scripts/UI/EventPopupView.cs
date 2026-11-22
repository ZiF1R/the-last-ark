using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupView : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;

	[Header("Text")]
	[SerializeField] TMP_Text titleText;
	[SerializeField] TMP_Text causeText;
	[SerializeField] TMP_Text bodyText;

	[Header("Choices")]
	[SerializeField] GameObject choiceBlock;
	[SerializeField] Button[] choiceButtons;
	[SerializeField] TMP_Text[] choiceLabels;

	[Header("Result")]
	[SerializeField] GameObject resultBlock;
	[SerializeField] TMP_Text resultText;
	[SerializeField] Button continueButton;

	EventDef _event;

	void Awake()
	{
		for (int i = 0; i < choiceButtons.Length; i++)
		{
			int index = i;
			choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
		}

		continueButton.onClick.AddListener(OnContinueClicked);
	}

	void OnEnable()
	{
		GameEvents.EventTriggered += Show;
		GameEvents.EventResolved += ShowResult;
		GameEvents.RunStarted += Hide;
	}

	void OnDisable()
	{
		GameEvents.EventTriggered -= Show;
		GameEvents.EventResolved -= ShowResult;
		GameEvents.RunStarted -= Hide;
	}

	void Show(EventDef e)
	{
		if (e == null) return;

		_event = e;

		bodyText.gameObject.SetActive(true);

		titleText.text = e.title;
		causeText.text = e.causeLine;
		bodyText.text = e.body;

		for (int i = 0; i < choiceButtons.Length; i++)
		{
			bool hasChoice = e.choices != null && i < e.choices.Length;

			choiceButtons[i].gameObject.SetActive(hasChoice);
			if (!hasChoice) continue;

			if (choiceLabels[i] != null)
				choiceLabels[i].text = e.choices[i].label;

			choiceButtons[i].interactable = true;
		}

		resultBlock.SetActive(false);
		choiceBlock.SetActive(true);

		panel.SetActive(true);
	}

	void OnChoiceClicked(int index)
	{
		if (_event == null || _event.choices == null || index >= _event.choices.Length)
			return;

		SetChoicesInteractable(false);
		flow.ChooseEvent(_event.choices[index]);
	}

	void ShowResult(Choice choice)
	{
		if (choice == null) return;

		choiceBlock.SetActive(false);

		bool hasResult = !string.IsNullOrEmpty(choice.result);
		resultText.text = choice.result;
		resultText.gameObject.SetActive(hasResult);

		bodyText.gameObject.SetActive(!hasResult);

		resultBlock.SetActive(true);
	}

	void OnContinueClicked()
	{
		_event = null;
		panel.SetActive(false);
		flow.ContinueAfterEvent();
	}

	void Hide(RunState _)
	{
		_event = null;
		panel.SetActive(false);
	}

	void SetChoicesInteractable(bool value)
	{
		foreach (Button button in choiceButtons)
			button.interactable = value;
	}
}
