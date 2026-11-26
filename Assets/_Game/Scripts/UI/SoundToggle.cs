using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The small sound button in the top bar. Toggles a compact panel with the
/// music/sfx sliders so volume is reachable mid-run, not only on the title.
/// The panel always starts closed and closes again when a run restarts.
/// </summary>
public class SoundToggle : MonoBehaviour
{
	[SerializeField] GameObject panel;
	[SerializeField] Button button;

	void Awake()
	{
		if (panel != null) panel.SetActive(false);
		if (button != null) button.onClick.AddListener(Toggle);
	}

	void OnEnable()
	{
		GameEvents.RunStarted += Hide;
		GameEvents.RunEnded += Hide;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= Hide;
		GameEvents.RunEnded -= Hide;
	}

	void Toggle()
	{
		if (panel == null) return;
		AudioManager.Play("click");
		panel.SetActive(!panel.activeSelf);
	}

	void Hide(RunState _) => panel.SetActive(false);
	void Hide(EndingDef _, RunState __) => panel.SetActive(false);
}
