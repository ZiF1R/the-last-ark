using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The opaque title panel that must exist before the first draft: it introduces the
/// premise and the three rules (pick one, six seats, zero ends the run).
/// It hides itself on RunStarted so a restart never bounces the player back here.
/// </summary>
public class TitleView : MonoBehaviour
{
	[SerializeField] GameFlow flow;
	[SerializeField] GameObject panel;
	[SerializeField] Button startButton;

	void Awake()
	{
		if (startButton != null)
			startButton.onClick.AddListener(OnStart);
	}

	void OnEnable() => GameEvents.RunStarted += Hide;
	void OnDisable() => GameEvents.RunStarted -= Hide;

	void OnStart()
	{
		if (startButton != null) startButton.interactable = false;
		AudioManager.Play("start");
		if (flow != null) flow.BeginRun();
	}

	void Hide(RunState _)
	{
		if (panel != null) panel.SetActive(false);
	}
}
