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
	[SerializeField] Button restartButton;

	void Awake() => restartButton.onClick.AddListener(flow.Restart);

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

	void Hide(RunState _) => panel.SetActive(false);

	void Show(EndingDef ending, RunState state)
	{
		panel.SetActive(true);
		titleText.text = ending.title;
		bodyText.text = ending.body;
		survivalText.text = state.survival.ToString();
		scienceText.text = state.science.ToString();
		moraleText.text = state.morale.ToString();
		leftBehindText.text = BuildLeftBehind(state);
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
}
