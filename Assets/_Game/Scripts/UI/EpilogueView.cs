using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpilogueView : MonoBehaviour
{
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;
	[SerializeField] TMP_Text titleText;
	[SerializeField] TMP_Text bodyText;
	[SerializeField] TMP_Text survivalText;
	[SerializeField] TMP_Text scienceText;
	[SerializeField] TMP_Text moraleText;
	[SerializeField] TMP_Text leftBehindText;
	[SerializeField] TMP_Text lostText;
	[SerializeField] Button restartButton;

	[Header("Sealing Beat")]
	[SerializeField] GameObject sealingPanel;
	[SerializeField] TMP_Text sealingText;
	[SerializeField] Button sealingButton;

	[Header("Juice")]
	[SerializeField] float sealCharsPerSecond = 34f;
	[SerializeField] float sealButtonDelay = 0.25f;
	[SerializeField] float revealFade = 0.22f;

	EndingDef _ending;
	RunState _state;

	void Awake()
	{
		if (restartButton != null)
			restartButton.onClick.AddListener(flow.Restart);

		if (sealingButton != null)
			sealingButton.onClick.AddListener(RevealEnding);
	}

	void OnEnable()
	{
		GameEvents.RunEnded += Show;
		GameEvents.RunStarted += Hide;
	}

	void OnDisable()
	{
		GameEvents.RunEnded -= Show;
		GameEvents.RunStarted -= Hide;
	}

	void Hide(RunState _)
	{
		StopAllCoroutines();

		panel.SetActive(false);
		if (sealingPanel != null) sealingPanel.SetActive(false);

		if (sealingButton != null) sealingButton.gameObject.SetActive(true);
		if (restartButton != null) restartButton.gameObject.SetActive(true);

		CanvasGroup group = panel.GetComponent<CanvasGroup>();
		if (group != null) group.alpha = 1f;

		if (sealingText != null) sealingText.maxVisibleCharacters = 99999;

		_ending = null;
		_state = null;
	}

	void Show(EndingDef ending, RunState state)
	{
		StopAllCoroutines();

		_ending = ending;
		_state = state;

		RenderEnding();

		if (sealingPanel != null)
		{
			if (sealingText != null)
				sealingText.text = BuildSealing(state);

			panel.SetActive(false);
			sealingPanel.SetActive(true);

			StartCoroutine(SealRoutine());
		}
		else
		{
			panel.SetActive(true);
			StartCoroutine(RevealRoutine());
		}
	}

	IEnumerator SealRoutine()
	{
		if (sealingButton != null) sealingButton.gameObject.SetActive(false);
		if (sealingText != null) sealingText.maxVisibleCharacters = 0;

		yield return Typewriter.Play(sealingText, sealCharsPerSecond);
		yield return new WaitForSecondsRealtime(sealButtonDelay);

		if (sealingButton != null) sealingButton.gameObject.SetActive(true);
	}

	void RevealEnding()
	{
		StopAllCoroutines();

		if (sealingPanel != null) sealingPanel.SetActive(false);

		panel.SetActive(true);
		StartCoroutine(RevealRoutine());
	}

	IEnumerator RevealRoutine()
	{
		CanvasGroup group = panel.GetComponent<CanvasGroup>();
		if (group == null) group = panel.AddComponent<CanvasGroup>();

		if (restartButton != null) restartButton.gameObject.SetActive(false);

		group.alpha = 0f;
		float t = 0f;

		while (t < revealFade)
		{
			t += Time.unscaledDeltaTime;
			group.alpha = Mathf.Clamp01(t / revealFade);
			yield return null;
		}

		group.alpha = 1f;

		if (restartButton != null)
		{
			yield return new WaitForSecondsRealtime(0.35f);
			restartButton.gameObject.SetActive(true);
		}
	}

	void RenderEnding()
	{
		if (_ending == null || _state == null) return;

		titleText.text = _ending.title;
		bodyText.text = _ending.body;
		survivalText.text = _state.survival.ToString();
		scienceText.text = _state.science.ToString();
		moraleText.text = _state.morale.ToString();
		leftBehindText.text = BuildLeftBehind(_state);

		if (lostText != null)
			lostText.text = BuildLost(_state);
	}

	static string BuildLeftBehind(RunState state)
	{
		var names = state.leftBehind
			.Select(c => c.displayName)
			.Distinct()
			.Reverse()
			.Take(3)
			.ToList();

		return names.Count == 0 ? "" : "LEFT BEHIND: " + string.Join(", ", names);
	}

	static string BuildLost(RunState state)
	{
		var names = state.lostCrew
			.Where(c => c != null)
			.Select(c => c.displayName)
			.Distinct()
			.ToList();

		return names.Count == 0 ? "" : "LOST: " + string.Join(", ", names);
	}

	static string BuildSealing(RunState state)
	{
		var names = state.lostCrew
			.Where(c => c != null)
			.Select(c => c.displayName)
			.Distinct()
			.ToList();

		return names.Count == 0
			? "THE DOOR CLOSES.\n\nThe ark is full. The rest of Earth is not."
			: "THE DOOR CLOSES.\n\n<size=50%>Lost: " + string.Join(", ", names) + ".</size>";
	}
}
