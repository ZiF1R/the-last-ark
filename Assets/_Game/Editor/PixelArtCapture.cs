using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders the UI to PNG files outside of play mode so the skin can be reviewed
/// without a human in the loop. Never saves the scene.
///   Unity.exe -batchmode -quit -projectPath . -executeMethod PixelArtCapture.CaptureFromBatch
/// </summary>
public static class PixelArtCapture
{
	const string ScenePath = "Assets/Scenes/MainScene.unity";
	const string OutDir = "Temp/Shots";

	static readonly List<GameObject> Spawned = new List<GameObject>();
	static Canvas _canvas;
	static RectTransform _cardRow;

	public static void CaptureFromBatch()
	{
		try
		{
			Shoot(1920, 1080, "1920");
			Shoot(1280, 720, "1280");
			Debug.Log("[Capture] done");
		}
		catch (Exception e)
		{
			Debug.LogError("[Capture] " + e);
		}
	}

	static void Shoot(int width, int height, string tag)
	{
		EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
		Directory.CreateDirectory(OutDir);

		_canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

		var scaler = _canvas.GetComponent<CanvasScaler>();
		var camGo = new GameObject("~ShotCam");
		var cam = camGo.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = height * 0.5f;
		cam.nearClipPlane = 0.3f;
		cam.farClipPlane = 100f;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = UITheme.Bg;
		cam.allowHDR = false;
		cam.allowMSAA = false;
		cam.transform.position = new Vector3(0f, 0f, -10f);

		var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
		cam.targetTexture = rt;

		_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		_canvas.worldCamera = cam;
		_canvas.planeDistance = 5f;

		// Mirror what CanvasScaler does at this resolution: the 1920x1080 canvas
		// is rasterised at width/1920, so a 1280 shot is what the browser shows.
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		scaler.scaleFactor = width / 1920f;

		// the runtime clears this on the first frame; in edit mode it still covers the screen
		SetAlpha("Fader", 0f);

		// ---- 1. title screen
		Shot(cam, rt, width, height, "01_title_" + tag);

		Panel("TitlePanel", false);

		// ---- 2. draft
		DressArk();
		SpawnCards();
		Shot(cam, rt, width, height, "02_draft_" + tag);

		// ---- 3. event
		Panel("EventPanel", true);
		DressEvent();
		Shot(cam, rt, width, height, "03_event_" + tag);
		DressEventResult();
		Shot(cam, rt, width, height, "04_result_" + tag);
		Panel("EventPanel", false);

		// ---- 4. epilogue
		Panel("EpiloguePanel", true);
		DressEpilogue();
		Shot(cam, rt, width, height, "05_epilogue_" + tag);
		Panel("EpiloguePanel", false);

		// ---- 5. sealing
		Panel("SealingPanel", true);
		SetText("UI/Canvas/SealingPanel/SealingText", "THE LIGHTS GO OUT");
		Shot(cam, rt, width, height, "06_sealing_" + tag);
		Panel("SealingPanel", false);

		foreach (GameObject go in Spawned)
			UnityEngine.Object.DestroyImmediate(go);
		Spawned.Clear();

		UnityEngine.Object.DestroyImmediate(camGo);
		UnityEngine.Object.DestroyImmediate(rt);
	}

	static void Shot(Camera cam, RenderTexture rt, int width, int height, string name)
	{
		Canvas.ForceUpdateCanvases();
		if (_cardRow != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_cardRow);
		Canvas.ForceUpdateCanvases();
		RefreshText();

		cam.Render();

		RenderTexture.active = rt;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();
		RenderTexture.active = null;

		string path = Path.Combine(OutDir, name + ".png");
		File.WriteAllBytes(path, tex.EncodeToPNG());
		UnityEngine.Object.DestroyImmediate(tex);

		Debug.Log("[Capture] wrote " + path);
	}

	// ---------------------------------------------------------------- helpers

	static Transform Find(string path)
	{
		string[] parts = path.Split('/');
		Transform current = null;

		foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
		{
			if (root.name != parts[0]) continue;
			current = root.transform;
			break;
		}

		if (current == null) return null;

		for (int i = 1; i < parts.Length; i++)
		{
			if (current == null) return null;
			current = current.Find(parts[i]);
		}

		return current;
	}

	static void SetText(string path, string text)
	{
		Transform t = Find(path);
		if (t == null) return;
		var tmp = t.GetComponent<TMP_Text>();
		if (tmp != null) tmp.text = text;
	}

	static void SetAlpha(string name, float alpha)
	{
		Transform fader = _canvas.transform.Find(name);
		if (fader == null) return;
		var img = fader.GetComponent<Image>();
		if (img == null) return;
		Color c = img.color;
		c.a = alpha;
		img.color = c;
	}

	static void Panel(string name, bool active)
	{
		Transform t = _canvas.transform.Find(name);
		if (t != null) t.gameObject.SetActive(active);
	}

	static void Show(string path, bool active)
	{
		Transform t = Find(path);
		if (t != null) t.gameObject.SetActive(active);
	}

	static void RefreshText()
	{
		foreach (TMP_Text text in _canvas.GetComponentsInChildren<TMP_Text>(true))
			text.ForceMeshUpdate(true, true);
	}

	static Sprite Icon(string name)
	{
		return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Icons/icon_" + name + ".png");
	}

	// ---------------------------------------------------------------- content

	static void SpawnCards()
	{
		_cardRow = Find("UI/Canvas/GameScreen/DraftView/DraftPanel/CardRow") as RectTransform;
		if (_cardRow == null) return;

		var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/CardView.prefab");
		var flow = UnityEngine.Object.FindAnyObjectByType<GameFlow>();
		if (prefab == null || flow == null || flow.db == null) return;

		List<CardDef> cards = flow.db.cards;
		int[] picks = { 0, 1, 2 };

		foreach (int index in picks)
		{
			if (index >= cards.Count) continue;

			GameObject go = (GameObject)UnityEngine.Object.Instantiate(prefab, _cardRow);
			go.name = "~Card" + index;
			Spawned.Add(go);
			DressCard(go.transform, cards[index]);
		}
	}

	static void DressCard(Transform card, CardDef def)
	{
		Transform name = card.Find("Name");
		if (name != null) name.GetComponent<TMP_Text>().text = def.displayName;

		Transform role = card.Find("Role");
		if (role != null) role.GetComponent<TMP_Text>().text = UITheme.RoleLabel(def.role);

		Transform quote = card.Find("Quote");
		if (quote != null) quote.GetComponent<TMP_Text>().text = def.quote;

		Transform band = card.Find("TopBand");
		if (band != null) band.GetComponent<Image>().color = UITheme.RoleColor(def.role);

		Transform icon = card.Find("Icon");
		if (icon != null)
		{
			var img = icon.GetComponent<Image>();
			img.sprite = Icon(RoleIconName(def.role));
			img.color = Color.white;
		}

		Transform effects = card.Find("Effects");
		if (effects != null)
		{
			var lines = new List<string>();
			AddEffect(lines, def.survival, "SURVIVAL", UITheme.Surv);
			AddEffect(lines, def.science, "SCIENCE", UITheme.Sci);
			AddEffect(lines, def.morale, "MORALE", UITheme.Mor);
			if (def.passiveSurvival != 0) AddEffect(lines, def.passiveSurvival, "SURVIVAL /TURN", UITheme.Surv);
			if (def.passiveScience != 0) AddEffect(lines, def.passiveScience, "SCIENCE /TURN", UITheme.Sci);
			if (def.passiveMorale != 0) AddEffect(lines, def.passiveMorale, "MORALE /TURN", UITheme.Mor);
			effects.GetComponent<TMP_Text>().text = string.Join("\n", lines);
		}

		Transform tags = card.Find("Tags");
		if (tags != null && def.tags != null)
		{
			var parts = new List<string>();
			foreach (CardTag tag in def.tags) parts.Add(tag.ToString().ToUpperInvariant());
			tags.GetComponent<TMP_Text>().text = string.Join("  ·  ", parts);
		}

		Transform cost = card.Find("CostBadge");
		if (cost != null)
		{
			cost.gameObject.SetActive(def.slotCost > 1);
			Transform costText = cost.Find("CostText");
			if (costText != null) costText.GetComponent<TMP_Text>().text = def.slotCost + " SLOTS";
		}
	}

	static void AddEffect(List<string> lines, int value, string label, Color color)
	{
		if (value == 0) return;
		lines.Add($"<color=#{UITheme.Html(color)}>{label} {(value > 0 ? "+" : string.Empty)}{value}</color>");
	}

	static string RoleIconName(CardRole role)
	{
		switch (role)
		{
			case CardRole.Biolog: return "biolog";
			case CardRole.Engineer: return "engineer";
			case CardRole.Soldier: return "soldier";
			case CardRole.Medic: return "medic";
			case CardRole.Child: return "child";
			case CardRole.Sick: return "sick";
			case CardRole.Farmer: return "farmer";
			default: return "mutant";
		}
	}

	static void DressArk()
	{
		Transform rail = Find("UI/Canvas/GameScreen/ArkRail");
		if (rail == null) return;

		Color empty = new Color(0.18f, 0.24f, 0.26f, 1f);
		string[] names = { "MAREK", "VERA", "TOBI" };
		CardRole[] roles = { CardRole.Soldier, CardRole.Biolog, CardRole.Child };

		for (int i = 1; i <= 6; i++)
		{
			Transform slot = rail.Find("Slot_" + i);
			if (slot == null) continue;

			var img = slot.GetComponent<Image>();
			Transform label = slot.Find("Label");
			var text = label != null ? label.GetComponent<TMP_Text>() : null;

			if (i <= names.Length)
			{
				Color tint = Color.Lerp(empty, UITheme.RoleColor(roles[i - 1]), 0.55f);
				if (img != null) img.color = tint;
				if (text != null)
				{
					text.text = names[i - 1];
					text.color = UITheme.RoleColor(roles[i - 1]);
				}
			}
			else
			{
				if (img != null) img.color = empty;
				if (text != null) text.text = string.Empty;
			}
		}
	}

	static void DressEvent()
	{
		var flow = UnityEngine.Object.FindAnyObjectByType<GameFlow>();
		if (flow == null || flow.db == null || flow.db.events.Count == 0) return;

		EventDef ev = flow.db.events[0];

		SetText("UI/Canvas/EventPanel/Dialog/Title", ev.title);
		SetText("UI/Canvas/EventPanel/Dialog/Cause", ev.causeLine);
		SetText("UI/Canvas/EventPanel/Dialog/Body", ev.body);

		for (int i = 0; i < 2; i++)
		{
			string letter = i == 0 ? "A" : "B";
			Transform choice = Find("UI/Canvas/EventPanel/Dialog/ChoiceBlock/Choice_" + letter);
			if (choice == null) continue;

			bool has = ev.choices != null && i < ev.choices.Length;
			choice.gameObject.SetActive(has);
			if (!has) continue;

			Transform label = choice.Find("Text (TMP)");
			if (label != null) label.GetComponent<TMP_Text>().text = ev.choices[i].label;
		}

		Transform result = Find("UI/Canvas/EventPanel/Dialog/ResultBlock");
		if (result != null) result.gameObject.SetActive(false);
	}

	static void DressEventResult()
	{
		var flow = UnityEngine.Object.FindAnyObjectByType<GameFlow>();
		if (flow == null || flow.db == null || flow.db.events.Count == 0) return;

		EventDef ev = flow.db.events[0];

		Show("UI/Canvas/EventPanel/Dialog/ChoiceBlock", false);
		Show("UI/Canvas/EventPanel/Dialog/ResultBlock", true);

		string body = ev.choices != null && ev.choices.Length > 0 ? ev.choices[0].result : string.Empty;
		string lost = $"<size={UITheme.FsMicro}><color=#{UITheme.Html(UITheme.Danger)}>LOST: ILYA</color></size>";
		SetText("UI/Canvas/EventPanel/Dialog/ResultBlock/ResultText",
			string.IsNullOrEmpty(body) ? lost : body + "\n\n" + lost);

		SetText("UI/Canvas/EventPanel/Dialog/ResultBlock/ContinueButton/Text (TMP)", "CONTINUE");
	}

	static void DressEpilogue()
	{
		var flow = UnityEngine.Object.FindAnyObjectByType<GameFlow>();
		if (flow == null || flow.db == null || flow.db.endings.Count == 0) return;

		EndingDef ending = flow.db.endings[0];

		SetText("UI/Canvas/EpiloguePanel/Content/Kicker", "RUN COMPLETE  ·  TURN 12");
		SetText("UI/Canvas/EpiloguePanel/Content/Title", ending.title);
		SetText("UI/Canvas/EpiloguePanel/Content/Body", ending.body);
		SetText("UI/Canvas/EpiloguePanel/Content/StatsRow/StatBlock_Survival/Value", "4");
		SetText("UI/Canvas/EpiloguePanel/Content/StatsRow/StatBlock_Science/Value", "7");
		SetText("UI/Canvas/EpiloguePanel/Content/StatsRow/StatBlock_Morale/Value", "2");
		SetText("UI/Canvas/EpiloguePanel/Content/LeftBehind", "LEFT BEHIND · MAREK · ROSA · KESTREL");
		Show("UI/Canvas/EpiloguePanel/Content/LeftBehind", true);
		SetText("UI/Canvas/EpiloguePanel/Content/LostText", "LOST ON THE WAY · ILYA");
	}
}


