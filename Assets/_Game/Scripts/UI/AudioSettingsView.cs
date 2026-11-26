using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Music/SFX sliders on the title panel. Values live in the AudioManager, which
/// persists them in PlayerPrefs, so this view only pushes changes across.
/// </summary>
public class AudioSettingsView : MonoBehaviour
{
	[SerializeField] Slider music;
	[SerializeField] Slider sfx;

	void Awake()
	{
		if (music != null)
			music.onValueChanged.AddListener(v =>
			{
				if (AudioManager.I != null)
					AudioManager.I.SetMusicVolume(v);
			});

		if (sfx != null)
			sfx.onValueChanged.AddListener(v =>
			{
				if (AudioManager.I != null)
					AudioManager.I.SetSfxVolume(v);
			});
	}

	void OnEnable()
	{
		// The sliders may be created before the AudioManager registers itself;
		// sync again every time the title screen comes back up.
		if (AudioManager.I == null) return;

		if (music != null) music.SetValueWithoutNotify(AudioManager.I.MusicVolume);
		if (sfx != null) sfx.SetValueWithoutNotify(AudioManager.I.SfxVolume);
	}
}
