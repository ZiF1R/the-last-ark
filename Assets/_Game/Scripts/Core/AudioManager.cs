using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Music + UI sound effects for the whole game. Lives on System/Audio in the scene.
/// Gameplay sounds are triggered through GameEvents, UI ones through the small
/// static helpers, so no gameplay script needs a direct reference to this class.
/// </summary>
public class AudioManager : MonoBehaviour
{
	[System.Serializable]
	public class SfxEntry
	{
		public string id;
		public AudioClip[] clips;
		[Range(0f, 1f)] public float volume = 1f;
		public float pitchMin = 0.97f;
		public float pitchMax = 1.03f;
	}

	public const string MusicKey = "ark.music";
	public const string SfxKey = "ark.sfx";

	public static AudioManager I { get; private set; }

	[Header("Music")]
	[SerializeField] AudioSource music;
	[SerializeField] AudioClip musicClip;

	[Header("SFX")]
	[SerializeField] AudioSource[] voices = new AudioSource[4];
	[SerializeField] SfxEntry[] entries;

	[Header("Defaults (first launch)")]
	[Range(0f, 1f)] [SerializeField] float defaultMusic = 0.3f;
	[Range(0f, 1f)] [SerializeField] float defaultSfx = 0.22f;

	readonly Dictionary<string, SfxEntry> _table = new();
	readonly Dictionary<string, float> _lastPlayed = new();
	int _nextVoice;
	int _lastCrew = -1;
	float _musicVol;
	float _sfxVol;

	void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(gameObject);
			return;
		}

		I = this;

		_musicVol = PlayerPrefs.GetFloat(MusicKey, defaultMusic);
		_sfxVol = PlayerPrefs.GetFloat(SfxKey, defaultSfx);

		if (music != null)
		{
			music.clip = musicClip;
			music.loop = true;
			music.playOnAwake = false;
			music.volume = _musicVol;
		}

		_table.Clear();

		if (entries != null)
			foreach (SfxEntry e in entries)
				if (e != null && !string.IsNullOrEmpty(e.id) && !_table.ContainsKey(e.id))
					_table[e.id] = e;
	}

	void OnDestroy()
	{
		if (I == this) I = null;
	}

	void OnEnable()
	{
		GameEvents.RunStarted += OnRunStarted;
		GameEvents.CrewChanged += OnCrewChanged;
		GameEvents.SynergyFired += OnSynergyFired;
		GameEvents.EventTriggered += OnEventTriggered;
		GameEvents.EventResolved += OnEventResolved;
		GameEvents.RunEnded += OnRunEnded;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= OnRunStarted;
		GameEvents.CrewChanged -= OnCrewChanged;
		GameEvents.SynergyFired -= OnSynergyFired;
		GameEvents.EventTriggered -= OnEventTriggered;
		GameEvents.EventResolved -= OnEventResolved;
		GameEvents.RunEnded -= OnRunEnded;
	}

	// ---------------------------------------------------------------- settings

	public float MusicVolume => _musicVol;
	public float SfxVolume => _sfxVol;

	public void SetMusicVolume(float v)
	{
		_musicVol = Mathf.Clamp01(v);
		PlayerPrefs.SetFloat(MusicKey, _musicVol);

		if (music != null) music.volume = _musicVol;
	}

	public void SetSfxVolume(float v)
	{
		_sfxVol = Mathf.Clamp01(v);
		PlayerPrefs.SetFloat(SfxKey, _sfxVol);
	}

	// ---------------------------------------------------------------- playing

	/// <summary>Plays a registered sound. Safe from anywhere; silently does nothing when the manager is absent.</summary>
	public static void Play(string id)
	{
		if (I == null) return;
		I.PlayInternal(id, 1f);
	}

	void PlayInternal(string id, float scale)
	{
		if (!_table.TryGetValue(id, out SfxEntry e) || e.clips == null || e.clips.Length == 0) return;
		if (voices == null || voices.Length == 0) return;

		// Never machine-gun one sound: sweeping the pointer across three cards
		// should stay a soft ripple, not a roll.
		float now = Time.unscaledTime;

		if (_lastPlayed.TryGetValue(id, out float last) && now - last < 0.05f) return;
		_lastPlayed[id] = now;

		AudioClip clip = e.clips[Random.Range(0, e.clips.Length)];
		if (clip == null) return;

		AudioSource voice = voices[_nextVoice];
		_nextVoice = (_nextVoice + 1) % voices.Length;

		voice.pitch = Random.Range(e.pitchMin, e.pitchMax);
		voice.PlayOneShot(clip, e.volume * _sfxVol * scale);
	}

	// ---------------------------------------------------------------- GameEvents

	void OnRunStarted(RunState state)
	{
		_lastCrew = state?.crew?.Count ?? 0;

		// Browsers only unlock audio on a user gesture; RunStarted always follows
		// the Start (or Sail Again) click, so this is the safe place to begin.
		if (music != null && musicClip != null && !music.isPlaying)
			music.Play();
	}

	/// <summary>One event covers both boarding and losing people; the crew count says which.</summary>
	void OnCrewChanged(RunState state)
	{
		int count = state?.crew?.Count ?? 0;

		if (_lastCrew < 0)
		{
			_lastCrew = count;
			return;
		}

		if (count > _lastCrew) PlayInternal("seat", 1f);
		else if (count < _lastCrew) PlayInternal("lost", 1f);

		_lastCrew = count;
	}

	void OnSynergyFired(SynergyDef _, StatDelta __, RunState ___) => PlayInternal("synergy", 1f);
	void OnEventTriggered(EventDef _, RunState ___) => PlayInternal("event", 1f);
	void OnEventResolved(Choice _, CardDef __) => PlayInternal("choice", 1f);
	void OnRunEnded(EndingDef _, RunState ___) => PlayInternal("end", 1f);
}
