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
		panel.SetActive(false);
		if (sealingPanel != null) sealingPanel.SetActive(false);

		_ending = null;
		_state = null;
	}

	void Show(EndingDef ending, RunState state)
	{
		_ending = ending;
		_state = state;

		RenderEnding();

		if (sealingPanel != null)
		{
			if (sealingText != null)
				sealingText.text = BuildSealing(state);

			panel.SetActive(false);
			sealingPanel.SetActive(true);
		}
		else
		{
			panel.SetActive(true);
		}
	}

	void RevealEnding()
	{
		if (sealingPanel != null) sealingPanel.SetActive(false);
		panel.SetActive(true);
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
