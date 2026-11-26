using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupView : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;
	[SerializeField] RectTransform dialog;

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

	[Header("Juice")]
	[SerializeField] float appearDuration = 0.14f;
	[SerializeField] float resultCharsPerSecond = 90f;
	[SerializeField] float maxResultDuration = 1.2f;

	CanvasGroup _dialogGroup;
	Coroutine _appear;
	Coroutine _typing;

	EventDef _event;

	void Awake()
	{
		for (int i = 0; i < choiceButtons.Length; i++)
		{
			int index = i;
			choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
		}

		continueButton.onClick.AddListener(OnContinueClicked);

		if (dialog == null && panel != null && panel.transform.childCount > 0)
			dialog = panel.transform.GetChild(0) as RectTransform;

		if (dialog != null)
		{
			_dialogGroup = dialog.GetComponent<CanvasGroup>();
			if (_dialogGroup == null)
				_dialogGroup = dialog.gameObject.AddComponent<CanvasGroup>();
		}
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

		StopAllCoroutines();
		_typing = null;
		_appear = null;
		bodyText.maxVisibleCharacters = 99999;
		resultText.maxVisibleCharacters = 99999;
		continueButton.interactable = true;

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
					choiceLabels[i].text = $"{ch.label}\n<size={UITheme.FsMicro}><color=#{UITheme.Html(hintColor)}>{hint}</color></size>";
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
		_appear = StartCoroutine(AppearRoutine());
	}

	IEnumerator AppearRoutine()
	{
		if (dialog == null || _dialogGroup == null) yield break;

		_dialogGroup.alpha = 0f;
		dialog.localScale = Vector3.one * 0.96f;

		float t = 0f;

		while (t < appearDuration)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / appearDuration));

			_dialogGroup.alpha = k;
			dialog.localScale = Vector3.Lerp(Vector3.one * 0.96f, Vector3.one, k);

			yield return null;
		}

		_dialogGroup.alpha = 1f;
		dialog.localScale = Vector3.one;
		_appear = null;
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
			string lostHex = UITheme.Html(UITheme.Danger);
			text = string.IsNullOrEmpty(text)
				? lostLine
				: $"{text}\n\n<size={UITheme.FsMicro}><color=#{lostHex}>{lostLine}</color></size>";
		}

		if (_typing != null)
		{
			StopCoroutine(_typing);
			_typing = null;
		}

		bool hasResult = !string.IsNullOrEmpty(text);

		string chosen = $"<size={UITheme.FsMicro}><color=#{UITheme.Html(UITheme.Muted)}>YOU CHOSE: {choice.label.ToUpperInvariant()}</color></size>";
		text = hasResult ? chosen + "\n\n" + text : chosen;

		resultText.text = text;
		resultText.gameObject.SetActive(true);
		// The body stays on screen on purpose: wiping it removes the context of the
		// choice that was just made.
		bodyText.gameObject.SetActive(true);

		resultBlock.SetActive(true);

		if (hasResult)
		{
			resultText.maxVisibleCharacters = 0;
			_typing = StartCoroutine(TypeResult());
		}
		else
		{
			continueButton.interactable = true;
		}
	}

	IEnumerator TypeResult()
	{
		continueButton.interactable = false;

		yield return Typewriter.Play(resultText, resultCharsPerSecond, maxResultDuration);

		continueButton.interactable = true;
		_typing = null;
	}

	void OnContinueClicked()
	{
		AudioManager.Play("click");

		_event = null;

		StopAllCoroutines();
		_typing = null;
		_appear = null;
		resultText.maxVisibleCharacters = 99999;

		panel.SetActive(false);
		flow.ContinueAfterEvent();
	}

	void Hide(RunState _)
	{
		_event = null;

		StopAllCoroutines();
		_typing = null;
		_appear = null;
		resultText.maxVisibleCharacters = 99999;
		_dialogGroup.alpha = 1f;

		panel.SetActive(false);
	}

	void SetChoicesInteractable(bool value)
	{
		foreach (Button button in choiceButtons)
			button.interactable = value;
	}
}
