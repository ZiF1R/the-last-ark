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

	[Header("Availability Colors")]
	[SerializeField] Color labelNormal = new Color(0.91f, 0.88f, 0.83f);
	[SerializeField] Color labelLocked = new Color(0.45f, 0.50f, 0.52f);
	[SerializeField] Color hintColor = new Color(0.55f, 0.62f, 0.64f);


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

	void Show(EventDef e, RunState state)
	{
		if (e == null) return;
		_event = e;

		titleText.text = e.title;
		causeText.text = e.causeLine;
		bodyText.text = e.body;
		bodyText.gameObject.SetActive(true);

		for (int i = 0; i < choiceButtons.Length; i++)
		{
			bool hasChoice = e.choices != null && i < e.choices.Length;
			choiceButtons[i].gameObject.SetActive(hasChoice);
			if (!hasChoice) continue;

			Choice ch = e.choices[i];
			if (ch == null)
			{
				choiceButtons[i].gameObject.SetActive(false);
				continue;
			}

			bool available = ch.IsAvailable(state);

			choiceButtons[i].interactable = available;

			if (choiceLabels != null && i < choiceLabels.Length)
			{
				if (available)
				{
					choiceLabels[i].text = ch.label;
					choiceLabels[i].color = labelNormal;
				}
				else
				{
					string hint = ch.GetLockHint(state);
					choiceLabels[i].text = $"{ch.label}\n<size=70%><color=#{ColorUtility.ToHtmlStringRGB(hintColor)}>{hint}</color></size>";
					choiceLabels[i].color = labelLocked;
				}
			}
		}

		var layout = choiceBlock.GetComponent<VerticalLayoutGroup>();
		if (layout != null)
			layout.childAlignment = e.choices.Length == 1
				? TextAnchor.MiddleCenter
				: TextAnchor.UpperCenter;

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

	void ShowResult(Choice choice, CardDef lost)
	{
		if (choice == null) return;

		choiceBlock.SetActive(false);

		string text = choice.result;

		if (lost != null)
		{
			string lostLine = $"LOST: {lost.displayName}";
			text = string.IsNullOrEmpty(text)
				? lostLine
				: $"{text}\n\n<size=85%><color=#B4544E>{lostLine}</color></size>";
		}

		bool hasResult = !string.IsNullOrEmpty(text);
		resultText.text = text;
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
