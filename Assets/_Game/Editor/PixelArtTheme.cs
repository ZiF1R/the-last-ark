using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// One-shot, idempotent skinning pass for The Last Ark.
/// Run from Tools/The Last Ark/Apply Pixel Theme or head-less:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod PixelArtTheme.ApplyFromBatch
/// Everything it writes is logged to Temp/ark_theme_report.txt.
/// </summary>
public static class PixelArtTheme
{
	const string ScenePath  = "Assets/Scenes/MainScene.unity";
	const string PrefabPath = "Assets/_Game/Prefabs/CardView.prefab";
	const string FontDir    = "Assets/_Game/Art/Fonts";
	const string UiDir      = "Assets/_Game/Art/Ui";
	const string IconDir    = "Assets/_Game/Art/Icons";
	const string ReportPath = "Logs/ark_theme_report.txt";

	static readonly StringBuilder Journal = new StringBuilder();

	static TMP_FontAsset _font;      // Silkscreen
	static TMP_FontAsset _fontBold;  // Silkscreen Bold
	static TMP_FontAsset _prose;     // VT323

	static Sprite _panel, _panelLit, _well, _btn, _btnDown, _btnSlate, _btnSlateDown, _seat, _divider, _stars;

	// ---------------------------------------------------------------- entry

	[MenuItem("Tools/The Last Ark/Apply Pixel Theme", false, 10)]
	public static void ApplyMenuItem() => Apply();

	public static void ApplyFromBatch() => Apply();

	static void Apply()
	{
		Journal.Clear();

		try
		{
			Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

			ReimportArt();
			LoadFonts();
			LoadSprites();

			ApplyCardPrefab();

			ApplyBackground(scene);
			ApplyTopBar(scene);
			ApplyDraft(scene);
			ApplyArk(scene);
			ApplyEvent(scene);
			ApplyEpilogue(scene);
			ApplySealing(scene);
			ApplyFeed(scene);
			ApplyFader(scene);
			ApplyTitle(scene);

			AssetDatabase.SaveAssets();
			EditorSceneManager.MarkSceneDirty(scene);
			EditorSceneManager.SaveScene(scene);

			Say("DONE");
		}
		catch (Exception e)
		{
			Say("EXCEPTION: " + e);
		}
		finally
		{
			File.WriteAllText(ReportPath, Journal.ToString());
			Debug.Log("[PixelArt]\n" + Journal);
		}
	}

	static void Say(string message)
	{
		Journal.AppendLine(message);
		Debug.Log("[PixelArt] " + message);
	}

	static void Warn(string message)
	{
		Journal.AppendLine("!! " + message);
		Debug.LogWarning("[PixelArt] " + message);
	}

	// ---------------------------------------------------------------- assets

	static void ReimportArt()
	{
		foreach (string folder in new[] { UiDir, IconDir, FontDir })
		{
			if (!AssetDatabase.IsValidFolder(folder))
			{
				Warn("missing folder " + folder);
				continue;
			}

			foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { folder }))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
				    path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
				{
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				}
			}
		}

		Say("art reimported");
	}

	static void LoadFonts()
	{
		_font = Font("Silkscreen-Regular.ttf", "Silkscreen-Regular");
		_fontBold = Font("Silkscreen-Bold.ttf", "Silkscreen-Bold");
		_prose = Font("VT323-Regular.ttf", "VT323-Regular");

		if (_font == null) _font = _fontBold;
		if (_fontBold == null) _fontBold = _font;
		if (_prose == null) _prose = _font;
	}

	static TMP_FontAsset Font(string ttfFile, string assetName)
	{
		string assetPath = FontDir + "/" + assetName + " SDF.asset";
		var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
		if (existing != null) return existing;

		var source = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "/" + ttfFile);
		if (source == null)
		{
			Warn("missing ttf " + ttfFile);
			return null;
		}

		var asset = TMP_FontAsset.CreateFontAsset(
			source, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);

		if (asset == null)
		{
			Warn("TMP font creation failed for " + ttfFile);
			return null;
		}

		asset.name = assetName + " SDF";
		AssetDatabase.CreateAsset(asset, assetPath);

		if (asset.material != null)
		{
			asset.material.name = asset.name + " Material";
			AssetDatabase.AddObjectToAsset(asset.material, asset);
		}

		if (asset.atlasTextures != null && asset.atlasTextures.Length > 0 && asset.atlasTextures[0] != null)
		{
			asset.atlasTextures[0].name = asset.name + " Atlas";
			AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
		}

		EditorUtility.SetDirty(asset);
		AssetDatabase.SaveAssets();
		Say("created font " + assetPath);

		return asset;
	}

	static void LoadSprites()
	{
		_panel = Sprite("panel_9s");
		_panelLit = Sprite("panel_9s_lit");
		_well = Sprite("well_9s");
		_btn = Sprite("btn_9s");
		_btnDown = Sprite("btn_9s_pressed");
		_btnSlate = Sprite("btn_slate_9s");
		_btnSlateDown = Sprite("btn_slate_9s_pressed");
		_seat = Sprite("seat_9s");
		_divider = Sprite("divider");
		_stars = Sprite("stars_tile");
	}

	static Sprite Sprite(string file)
	{
		var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiDir + "/" + file + ".png");
		if (sprite == null) Warn("missing sprite " + file);
		return sprite;
	}

	static Sprite Icon(string name)
	{
		var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconDir + "/icon_" + name + ".png");
		if (sprite == null) Warn("missing icon " + name);
		return sprite;
	}

	// ---------------------------------------------------------------- helpers

	static Transform Find(string path)
	{
		string[] parts = path.Split('/');
		Transform current = null;

		foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
		{
			if (root.name != parts[0]) continue;
			current = root.transform;
			break;
		}

		if (current == null)
		{
			Warn("missing root " + path);
			return null;
		}

		for (int i = 1; i < parts.Length; i++)
		{
			current = current.Find(parts[i]);
			if (current == null)
			{
				Warn("missing node " + path);
				return null;
			}
		}

		return current;
	}

	static T Get<T>(string path) where T : Component
	{
		Transform t = Find(path);
		return t == null ? null : t.GetComponent<T>();
	}

	static RectTransform Rect(string path)
	{
		Transform t = Find(path);
		return t as RectTransform;
	}

	/// <summary>Anchor/pivot the rect to a point with a fixed size.</summary>
	static void Box(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
	{
		if (rt == null) return;
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.pivot = pivot;
		rt.anchoredPosition = position;
		rt.sizeDelta = size;
		rt.localScale = Vector3.one;
	}

	/// <summary>Full-bleed child of its parent.</summary>
	static void Stretch(RectTransform rt, Vector4 margin)
	{
		if (rt == null) return;
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.offsetMin = new Vector2(margin.x, margin.y);
		rt.offsetMax = new Vector2(-margin.z, -margin.w);
	}

	/// <summary>Pinned to the top edge, y is the distance from that edge.</summary>
	static void FromTop(RectTransform rt, float y, float width, float height)
	{
		Box(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(width, height));
	}

	/// <summary>Pinned to the bottom edge, y is the distance from that edge.</summary>
	static void FromBottom(RectTransform rt, float y, float width, float height)
	{
		Box(rt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(width, height));
	}

	static void SkinText(TMP_Text text, TMP_FontAsset font, float size, Color color,
		TextAlignmentOptions align, bool wrap = false, bool ellipsis = false)
	{
		if (text == null) return;

		if (font != null) text.font = font;

		text.fontSize = size;
		// Never let TMP fake a style: synthetic bold/italic both smears the pixel
		// glyphs and, on an already-bold SDF asset, ends up rendering nothing.
		text.fontStyle = FontStyles.Normal;
		text.color = color;
		text.alignment = align;
		text.enableAutoSizing = false;
		text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
		text.overflowMode = ellipsis ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
		text.raycastTarget = false;
		text.richText = true;

		// A box shorter than a single line of its font plus ellipsis overflow
		// makes TMP drop the line entirely instead of truncating it (this is
		// how the card title went missing). Give such boxes the line they need.
		if (ellipsis && font != null)
		{
			var rt = text.rectTransform;
			float line = size * (font.faceInfo.lineHeight / font.faceInfo.pointSize);

			if (rt.rect.height + 0.01f < line)
			{
				if (rt.pivot.y > 0.99f)
				{
					Vector2 sizeDelta = rt.sizeDelta;
					sizeDelta.y = line;
					rt.sizeDelta = sizeDelta;
				}
				else
				{
					Say("text box too short: " + text.name + " " + rt.rect.height.ToString("0") + " < " + line.ToString("0"));
				}
			}
		}

		// TMP keeps a second, serialized copy of the colour and builds the glyph
		// vertices from that one. .color only writes the first copy, so a prefab
		// or scene saved by an editor script would bake the previous colour in.
		Color32 c = color;
		var so = new SerializedObject(text);
		SerializedProperty rgba = so.FindProperty("m_fontColor32")?.FindPropertyRelative("rgba");

		if (rgba != null)
		{
			rgba.uintValue = (uint)(c.r | (c.g << 8) | (c.b << 16) | (c.a << 24));
			so.ApplyModifiedPropertiesWithoutUndo();
		}
		else
		{
			var field = typeof(TMP_Text).GetField("m_fontColor32",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			field?.SetValue(text, c);
		}
	}

	/// <summary>Slice a sprite at exactly 4 canvas units per source pixel.</summary>
	static void SkinImage(Image image, Sprite sprite, Color color, Image.Type type = Image.Type.Sliced)
	{
		if (image == null) return;

		image.sprite = sprite;
		image.color = color;
		image.type = sprite != null && sprite.border != Vector4.zero ? type : Image.Type.Simple;
		image.pixelsPerUnitMultiplier = 0.25f;
		image.preserveAspect = false;
	}

	static void SkinButton(Button button, Sprite normal, Sprite pressed)
	{
		if (button == null) return;

		button.transition = Selectable.Transition.SpriteSwap;
		button.spriteState = new SpriteState
		{
			highlightedSprite = normal,
			pressedSprite = pressed,
			selectedSprite = normal,
			disabledSprite = normal
		};

		ColorBlock colors = button.colors;
		colors.normalColor = Color.white;
		colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
		colors.pressedColor = Color.white;
		colors.selectedColor = Color.white;
		colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);
		colors.fadeDuration = 0.06f;
		button.colors = colors;
	}

	static GameObject Node(string name, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
		go.transform.SetParent(parent, false);
		return go;
	}

	static GameObject Existing(Transform parent, string name)
	{
		Transform child = parent.Find(name);
		return child != null ? child.gameObject : Node(name, parent);
	}

	/// <summary>
	/// Replaces a text component with a brand new one. A TMP component that was
	/// only ever edited in the inspector can end up in a state where a script
	/// cannot make it build any glyphs (the card title rendered nothing at all
	/// while a fresh component with identical settings worked), so the prefab
	/// gets a clean component instead of a patched one.
	/// </summary>
	static TMP_Text Fresh(Transform host)
	{
		var old = host.GetComponent<TMP_Text>();
		if (old != null) UnityEngine.Object.DestroyImmediate(old, true);
		return host.gameObject.AddComponent<TextMeshProUGUI>();
	}

	// ---------------------------------------------------------------- card prefab

	static void ApplyCardPrefab()
	{
		GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);

		try
		{
			var rt = (RectTransform)root.transform;
			rt.sizeDelta = new Vector2(400f, 568f);

			var element = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
			element.preferredWidth = 400f;
			element.preferredHeight = 568f;

			var frame = root.GetComponent<Image>();
			SkinImage(frame, _panel, Color.white);

			string[] roles = { "biolog", "engineer", "soldier", "medic", "child", "sick", "farmer", "mutant" };

			// ---- top band
			Transform bandT = root.transform.Find("TopBand");
			if (bandT != null)
			{
				FromTop((RectTransform)bandT, 0f, 400f, 8f);
				var bandImg = bandT.GetComponent<Image>();
				if (bandImg != null)
				{
					bandImg.sprite = null;
					bandImg.type = Image.Type.Simple;
					bandImg.color = UITheme.Mor;
					bandImg.raycastTarget = false;
				}
			}

			// ---- icon
			Transform iconT = root.transform.Find("Icon");
			if (iconT != null)
			{
				FromTop((RectTransform)iconT, 24f, 128f, 128f);
				var img = iconT.GetComponent<Image>();
				SkinImage(img, Icon("biolog"), Color.white, Image.Type.Simple);
				img.preserveAspect = true;
				img.raycastTarget = false;
			}

			// ---- name
			Transform nameT = root.transform.Find("Name");
			TMP_Text nameLabel = null;
			if (nameT != null)
			{
				// 48pt Silkscreen needs a 61px line box; anything shorter makes TMP
				// truncate the line to nothing before it ever reaches the screen.
				FromTop((RectTransform)nameT, 160f, 368f, 64f);
				nameLabel = Fresh(nameT);
				SkinText(nameLabel, _fontBold, UITheme.FsName, UITheme.Bone,
					TextAlignmentOptions.Center, false, false);
				nameLabel.text = "NAME";
			}

			// ---- role
			Transform roleT = root.transform.Find("Role");
			TMP_Text roleLabel = null;
			if (roleT != null)
			{
				FromTop((RectTransform)roleT, 224f, 368f, 24f);
				roleLabel = Fresh(roleT);
				SkinText(roleLabel, _font, UITheme.FsMicro, UITheme.Muted,
					TextAlignmentOptions.Center, false, true);
				roleLabel.text = "ROLE";
			}

			// ---- divider
			Transform divT = root.transform.Find("Divider");
			if (divT != null)
			{
				FromTop((RectTransform)divT, 256f, 336f, 4f);
				SkinImage(divT.GetComponent<Image>(), _divider, UITheme.Line, Image.Type.Simple);
			}

			// ---- quote
			Transform quoteT = root.transform.Find("Quote");
			TMP_Text quoteLabel = null;
			if (quoteT != null)
			{
				FromTop((RectTransform)quoteT, 264f, 336f, 92f);
				quoteLabel = Fresh(quoteT);
				SkinText(quoteLabel, _prose, UITheme.FsProse, UITheme.Muted,
					TextAlignmentOptions.Top, true, true);
				quoteLabel.text = "He keeps the lights on. For now.";
			}

			// ---- tags
			Transform tagsT = root.transform.Find("Tags");
			TMP_Text tagsLabel = null;
			if (tagsT != null)
			{
				FromTop((RectTransform)tagsT, 362f, 368f, 24f);
				tagsLabel = Fresh(tagsT);
				SkinText(tagsLabel, _font, UITheme.FsMicro, UITheme.Muted,
					TextAlignmentOptions.Center, false, true);
				tagsLabel.text = "TAG";
			}

			// ---- effects
			Transform effectsT = root.transform.Find("Effects");
			TMP_Text effectsLabel = null;
			if (effectsT != null)
			{
				FromBottom((RectTransform)effectsT, 16f, 368f, 160f);
				effectsLabel = Fresh(effectsT);
				SkinText(effectsLabel, _font, UITheme.FsBody, UITheme.Bone,
					TextAlignmentOptions.TopLeft, true, false);
				effectsLabel.text = "+1 SURVIVAL\n-1 MORALE";
			}

			// ---- cost badge
			Transform badgeT = root.transform.Find("CostBadge");
			TMP_Text costLabel = null;
			if (badgeT != null)
			{
				Box((RectTransform)badgeT, new Vector2(1f, 1f), new Vector2(1f, 1f),
					new Vector2(-12f, -12f), new Vector2(112f, 32f));

				var badgeImg = badgeT.GetComponent<Image>();
				SkinImage(badgeImg, _btn, Color.white);
				badgeImg.raycastTarget = false;

				Transform costT = badgeT.Find("CostText");
				if (costT != null)
				{
					Stretch((RectTransform)costT, Vector4.zero);
					costLabel = Fresh(costT);
					SkinText(costLabel, _font, UITheme.FsMicro, UITheme.Bg,
						TextAlignmentOptions.Center, false, true);
					costLabel.text = "2 SLOTS";
				}
			}

			// ---- point CardView at the rebuilt components
			var view = root.GetComponent<CardView>();
			if (view != null)
			{
				var so = new SerializedObject(view);
				so.FindProperty("hoverFrame").objectReferenceValue = _panelLit;

				var roleIcons = so.FindProperty("roleIcons");
				roleIcons.arraySize = roles.Length;
				for (int i = 0; i < roles.Length; i++)
					roleIcons.GetArrayElementAtIndex(i).objectReferenceValue = Icon(roles[i]);

				Transform band = root.transform.Find("TopBand");
				if (band != null)
					so.FindProperty("topBand").objectReferenceValue = band.GetComponent<Image>();

				so.FindProperty("nameText").objectReferenceValue = nameLabel;
				so.FindProperty("roleText").objectReferenceValue = roleLabel;
				so.FindProperty("quoteText").objectReferenceValue = quoteLabel;
				so.FindProperty("tagsText").objectReferenceValue = tagsLabel;
				so.FindProperty("effectsText").objectReferenceValue = effectsLabel;
				so.FindProperty("costText").objectReferenceValue = costLabel;

				so.ApplyModifiedPropertiesWithoutUndo();
			}

			Say("card prefab 400x600");
			PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
		}
		finally
		{
			PrefabUtility.UnloadPrefabContents(root);
		}
	}

	// ---------------------------------------------------------------- scene

	static void ApplyBackground(Scene scene)
	{
		RectTransform bg = Rect("UI/Canvas/BG");
		if (bg == null) return;

		Stretch(bg, Vector4.zero);
		var img = bg.GetComponent<Image>();
		if (img != null)
		{
			img.sprite = null;
			img.type = Image.Type.Simple;
			img.color = UITheme.Bg;
			img.raycastTarget = false;
		}

		GameObject stars = Existing(bg, "Stars");
		var srt = (RectTransform)stars.transform;
		Stretch(srt, Vector4.zero);
		var simg = stars.GetComponent<Image>();
		if (simg == null) simg = stars.AddComponent<Image>();
		simg.sprite = _stars;
		simg.type = Image.Type.Tiled;
		simg.pixelsPerUnitMultiplier = 0.25f;
		simg.color = new Color(1f, 1f, 1f, 0.7f);
		simg.raycastTarget = false;

		Say("background");
	}

	static void ApplyTopBar(Scene scene)
	{
		RectTransform bar = Rect("UI/Canvas/GameScreen/TopBar");
		if (bar == null) return;

		Box(bar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 96f));
		bar.anchorMin = new Vector2(0f, 1f);
		bar.anchorMax = new Vector2(1f, 1f);
		bar.sizeDelta = new Vector2(0f, 96f);

		var img = bar.GetComponent<Image>();
		if (img != null)
		{
			img.sprite = null;
			img.type = Image.Type.Simple;
			img.color = UITheme.Panel;
			img.raycastTarget = false;
		}

		// hairline under the bar
		GameObject edge = Existing(bar, "Edge");
		var ert = (RectTransform)edge.transform;
		ert.anchorMin = new Vector2(0f, 0f);
		ert.anchorMax = new Vector2(1f, 0f);
		ert.pivot = new Vector2(0.5f, 0f);
		ert.anchoredPosition = Vector2.zero;
		ert.sizeDelta = new Vector2(0f, 4f);
		var eimg = edge.GetComponent<Image>();
		if (eimg == null) eimg = edge.AddComponent<Image>();
		eimg.sprite = _divider;
		eimg.type = Image.Type.Simple;
		eimg.pixelsPerUnitMultiplier = 0.25f;
		eimg.color = UITheme.Line;
		eimg.raycastTarget = false;

		// ---- stats
		RectTransform stats = Rect("UI/Canvas/GameScreen/TopBar/Stats");
		if (stats != null)
		{
			Box(stats, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(760f, 64f));

			var layout = stats.GetComponent<HorizontalLayoutGroup>();
			if (layout == null) layout = stats.gameObject.AddComponent<HorizontalLayoutGroup>();
			layout.spacing = 32f;
			layout.childAlignment = TextAnchor.MiddleLeft;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			layout.padding = new RectOffset(0, 0, 0, 0);

			SkinStatItem("UI/Canvas/GameScreen/TopBar/Stats/StatItem_Survival", "surv", UITheme.Surv);
			SkinStatItem("UI/Canvas/GameScreen/TopBar/Stats/StatItem_Science", "sci", UITheme.Sci);
			SkinStatItem("UI/Canvas/GameScreen/TopBar/Stats/StatItem_Morale", "mor", UITheme.Mor);
		}

		// ---- turn label
		RectTransform turn = Rect("UI/Canvas/GameScreen/TopBar/TurnLabel");
		if (turn != null)
		{
			Box(turn, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-48f, 20f), new Vector2(430f, 40f));
			SkinText(turn.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Bone,
				TextAlignmentOptions.Right);
		}

		// ---- pending effects, centred between the stats and the turn counter
		RectTransform pending = Rect("UI/Canvas/GameScreen/TopBar/PendingText");
		if (pending != null)
		{
			Box(pending, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(620f, 64f));
			SkinText(pending.GetComponent<TMP_Text>(), _font, UITheme.FsMicro, UITheme.Mor,
				TextAlignmentOptions.Center, true);
		}

		Say("top bar");
	}

	static void SkinStatItem(string path, string icon, Color color)
	{
		RectTransform item = Rect(path);
		if (item == null) return;

		var element = item.GetComponent<LayoutElement>() ?? item.gameObject.AddComponent<LayoutElement>();
		element.preferredWidth = 192f;
		element.preferredHeight = 64f;

		var layout = item.GetComponent<HorizontalLayoutGroup>();
		if (layout == null) layout = item.gameObject.AddComponent<HorizontalLayoutGroup>();
		layout.spacing = 8f;
		layout.childAlignment = TextAnchor.MiddleLeft;
		layout.childControlWidth = true;
		layout.childControlHeight = true;
		layout.childForceExpandWidth = false;
		layout.childForceExpandHeight = false;
		layout.padding = new RectOffset(0, 0, 0, 0);

		Transform iconT = item.Find("Image");
		if (iconT != null)
		{
			iconT.SetSiblingIndex(0);
			var iconElement = iconT.GetComponent<LayoutElement>() ?? iconT.gameObject.AddComponent<LayoutElement>();
			iconElement.preferredWidth = 64f;
			iconElement.preferredHeight = 64f;

			var img = iconT.GetComponent<Image>();
			SkinImage(img, Icon(icon + "_16"), color, Image.Type.Simple);
			img.preserveAspect = true;
			img.raycastTarget = false;
		}

		Transform valueT = item.Find("Value");
		if (valueT != null)
		{
			var valueElement = valueT.GetComponent<LayoutElement>() ?? valueT.gameObject.AddComponent<LayoutElement>();
			valueElement.preferredWidth = 120f;
			valueElement.preferredHeight = 64f;

			SkinText(valueT.GetComponent<TMP_Text>(), _font, UITheme.FsName, color, TextAlignmentOptions.Left);
		}
	}

	static void ApplyDraft(Scene scene)
	{
		RectTransform row = Rect("UI/Canvas/GameScreen/DraftView/DraftPanel/CardRow");
		if (row != null)
		{
			Box(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1312f, 600f));

			var layout = row.GetComponent<HorizontalLayoutGroup>();
			if (layout != null)
			{
				layout.spacing = 56f;
				layout.padding = new RectOffset(0, 0, 0, 0);
				layout.childAlignment = TextAnchor.MiddleCenter;
				layout.childControlWidth = true;
				layout.childControlHeight = true;
				layout.childForceExpandWidth = false;
				layout.childForceExpandHeight = false;
			}
		}

		RectTransform prompt = Rect("UI/Canvas/GameScreen/DraftView/DraftPanel/Prompt");
		if (prompt != null)
		{
			// The turn counter already says how many slots are free; a second
			// instruction line only competes with the synergy toast for this band.
			prompt.gameObject.SetActive(false);
		}

		Say("draft row 1312x600");
	}

	static void ApplyArk(Scene scene)
	{
		RectTransform rail = Rect("UI/Canvas/GameScreen/ArkRail");
		if (rail == null) return;

		FromBottom(rail, 40f, 832f, 176f);

		var img = rail.GetComponent<Image>();
		SkinImage(img, _well, Color.white);

		var layout = rail.GetComponent<HorizontalLayoutGroup>();
		if (layout != null)
		{
			layout.padding = new RectOffset(16, 16, 16, 16);
			layout.spacing = 16f;
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childControlWidth = true;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = true;
		}

		var view = rail.GetComponent<SlotRailView>();
		var so = view != null ? new SerializedObject(view) : null;

		if (so != null)
		{
			ColorBlockGuard(so);
			so.ApplyModifiedPropertiesWithoutUndo();
		}

		var labels = new TMP_Text[6];

		for (int i = 1; i <= 6; i++)
		{
			RectTransform slot = Rect("UI/Canvas/GameScreen/ArkRail/Slot_" + i);
			if (slot == null) continue;

			slot.sizeDelta = new Vector2(120f, 144f);

			var element = slot.GetComponent<LayoutElement>() ?? slot.gameObject.AddComponent<LayoutElement>();
			element.preferredWidth = 120f;
			element.preferredHeight = 144f;

			var slotImage = slot.GetComponent<Image>();
			SkinImage(slotImage, _seat, Color.white);
			if (slotImage != null) slotImage.raycastTarget = false;

			GameObject labelGo = Existing(slot, "Label");
			var labelRt = (RectTransform)labelGo.transform;
			Stretch(labelRt, new Vector4(4f, 4f, 4f, 4f));

			var label = labelGo.GetComponent<TMP_Text>();
			if (label == null) label = labelGo.AddComponent<TextMeshProUGUI>();
			SkinText(label, _font, UITheme.FsMicro, UITheme.Bone, TextAlignmentOptions.Center, true, true);

			labels[i - 1] = label;
		}

		if (so != null)
		{
			var prop = so.FindProperty("slotLabels");
			prop.arraySize = labels.Length;
			for (int i = 0; i < labels.Length; i++)
				prop.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];

			so.FindProperty("roleTint").floatValue = 0.55f;
			so.ApplyModifiedPropertiesWithoutUndo();
		}

		// caption above the rail
		Transform screen = Find("UI/Canvas/GameScreen");
		if (screen != null)
		{
			GameObject caption = Existing(screen, "ArkCaption");
			var crt = (RectTransform)caption.transform;
			FromBottom(crt, 224f, 600f, 24f);

			var text = caption.GetComponent<TMP_Text>();
			if (text == null) text = caption.AddComponent<TextMeshProUGUI>();
			SkinText(text, _font, UITheme.FsMicro, UITheme.Muted, TextAlignmentOptions.Center);
			text.text = "THE ARK";
		}

		Say("ark rail 832x176 with 6 labelled seats");
	}

	static void ColorBlockGuard(SerializedObject so)
	{
		var empty = so.FindProperty("emptyColor");
		if (empty != null) empty.colorValue = new Color(0.18f, 0.24f, 0.26f, 1f);

		var sealedC = so.FindProperty("sealedColor");
		if (sealedC != null) sealedC.colorValue = new Color(0.35f, 0.13f, 0.10f, 1f);
	}

	static void ApplyEvent(Scene scene)
	{
		RectTransform dialog = Rect("UI/Canvas/EventPanel/Dialog");
		if (dialog == null) return;

		Box(dialog, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100f, 540f));
		SkinImage(dialog.GetComponent<Image>(), _panel, Color.white);

		RectTransform dim = Rect("UI/Canvas/EventPanel");
		var dimImg = dim != null ? dim.GetComponent<Image>() : null;
		if (dimImg != null)
		{
			dimImg.sprite = null;
			dimImg.type = Image.Type.Simple;
			dimImg.color = new Color(0.02f, 0.04f, 0.05f, 0.86f);
		}

		RectTransform band = Rect("UI/Canvas/EventPanel/Dialog/TopBand");
		if (band != null)
		{
			band.anchorMin = new Vector2(0f, 1f);
			band.anchorMax = new Vector2(1f, 1f);
			band.pivot = new Vector2(0.5f, 1f);
			band.anchoredPosition = Vector2.zero;
			band.sizeDelta = new Vector2(0f, 4f);

			var img = band.GetComponent<Image>();
			if (img != null)
			{
				img.sprite = null;
				img.type = Image.Type.Simple;
				img.color = UITheme.Mor;
				img.raycastTarget = false;
			}
		}

		RectTransform title = Rect("UI/Canvas/EventPanel/Dialog/Title");
		if (title != null)
		{
			FromTop(title, 24f, 960f, 64f);
			SkinText(title.GetComponent<TMP_Text>(), _fontBold, UITheme.FsName, UITheme.Bone,
				TextAlignmentOptions.Center, false, true);
		}

		RectTransform cause = Rect("UI/Canvas/EventPanel/Dialog/Cause");
		if (cause != null)
		{
			FromTop(cause, 96f, 960f, 24f);
			SkinText(cause.GetComponent<TMP_Text>(), _font, UITheme.FsMicro, UITheme.Muted,
				TextAlignmentOptions.Center, false, true);
		}

		RectTransform div = Rect("UI/Canvas/EventPanel/Dialog/Divider");
		if (div != null)
		{
			FromTop(div, 132f, 900f, 4f);
			SkinImage(div.GetComponent<Image>(), _divider, UITheme.Line, Image.Type.Simple);
		}

		RectTransform body = Rect("UI/Canvas/EventPanel/Dialog/Body");
		if (body != null)
		{
			FromTop(body, 148f, 940f, 130f);
			SkinText(body.GetComponent<TMP_Text>(), _prose, UITheme.FsProse, UITheme.Bone,
				TextAlignmentOptions.TopLeft, true);
		}

		RectTransform choices = Rect("UI/Canvas/EventPanel/Dialog/ChoiceBlock");
		if (choices != null)
		{
			FromTop(choices, 288f, 980f, 208f);

			var layout = choices.GetComponent<VerticalLayoutGroup>();
			if (layout != null)
			{
				layout.spacing = 16f;
				layout.padding = new RectOffset(0, 0, 0, 0);
				layout.childAlignment = TextAnchor.UpperCenter;
				layout.childControlWidth = true;
				layout.childControlHeight = true;
				layout.childForceExpandWidth = false;
				layout.childForceExpandHeight = false;
			}
		}

		for (int i = 0; i < 2; i++)
		{
			string letter = i == 0 ? "A" : "B";
			RectTransform choice = Rect("UI/Canvas/EventPanel/Dialog/ChoiceBlock/Choice_" + letter);
			if (choice == null) continue;

			choice.sizeDelta = new Vector2(980f, 96f);

			var element = choice.GetComponent<LayoutElement>() ?? choice.gameObject.AddComponent<LayoutElement>();
			element.preferredWidth = 980f;
			element.preferredHeight = 96f;

			SkinImage(choice.GetComponent<Image>(), _btnSlate, Color.white);
			SkinButton(choice.GetComponent<Button>(), _btnSlate, _btnSlateDown);

			Transform label = choice.Find("Text (TMP)");
			if (label != null)
			{
				Stretch((RectTransform)label, new Vector4(28f, 8f, 28f, 8f));
				SkinText(label.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Bone,
					TextAlignmentOptions.Left, true);
			}
		}

		RectTransform result = Rect("UI/Canvas/EventPanel/Dialog/ResultBlock");
		if (result != null)
			FromTop(result, 288f, 980f, 232f);

		RectTransform resultText = Rect("UI/Canvas/EventPanel/Dialog/ResultBlock/ResultText");
		if (resultText != null)
		{
			FromTop(resultText, 0f, 940f, 156f);
			SkinText(resultText.GetComponent<TMP_Text>(), _prose, UITheme.FsProse, UITheme.Bone,
				TextAlignmentOptions.TopLeft, true);
		}

		RectTransform cont = Rect("UI/Canvas/EventPanel/Dialog/ResultBlock/ContinueButton");
		if (cont != null)
		{
			FromTop(cont, 164f, 300f, 64f);
			SkinImage(cont.GetComponent<Image>(), _btn, Color.white);
			SkinButton(cont.GetComponent<Button>(), _btn, _btnDown);

			Transform label = cont.Find("Text (TMP)");
			if (label != null)
			{
				Stretch((RectTransform)label, new Vector4(8f, 4f, 8f, 4f));
				SkinText(label.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Bg,
					TextAlignmentOptions.Center);
			}
		}

		Say("event dialog 1100x540");
	}

	static void ApplyEpilogue(Scene scene)
	{
		RectTransform content = Rect("UI/Canvas/EpiloguePanel/Content");
		if (content == null) return;

		Box(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1280f, 800f));

		RectTransform kicker = Rect("UI/Canvas/EpiloguePanel/Content/Kicker");
		if (kicker != null)
		{
			FromTop(kicker, 8f, 1100f, 24f);
			SkinText(kicker.GetComponent<TMP_Text>(), _font, UITheme.FsMicro, UITheme.Muted,
				TextAlignmentOptions.Center);
		}

		RectTransform title = Rect("UI/Canvas/EpiloguePanel/Content/Title");
		if (title != null)
		{
			FromTop(title, 44f, 1100f, 100f);
			SkinText(title.GetComponent<TMP_Text>(), _fontBold, UITheme.FsTitle, UITheme.Bone,
				TextAlignmentOptions.Center, false, true);
		}

		RectTransform div = Rect("UI/Canvas/EpiloguePanel/Content/Divider");
		if (div != null)
		{
			FromTop(div, 160f, 480f, 4f);
			SkinImage(div.GetComponent<Image>(), _divider, UITheme.Line, Image.Type.Simple);
		}

		RectTransform body = Rect("UI/Canvas/EpiloguePanel/Content/Body");
		if (body != null)
		{
			FromTop(body, 180f, 900f, 260f);
			SkinText(body.GetComponent<TMP_Text>(), _prose, UITheme.FsProse, UITheme.Muted,
				TextAlignmentOptions.Top, true);
		}

		RectTransform statsRow = Rect("UI/Canvas/EpiloguePanel/Content/StatsRow");
		if (statsRow != null)
			FromTop(statsRow, 470f, 1000f, 90f);

		SkinEpilogueStat("Survival", -340f, UITheme.Surv);
		SkinEpilogueStat("Science", 0f, UITheme.Sci);
		SkinEpilogueStat("Morale", 340f, UITheme.Mor);

		RectTransform left = Rect("UI/Canvas/EpiloguePanel/Content/LeftBehind");
		if (left != null)
		{
			FromTop(left, 580f, 1100f, 24f);
			SkinText(left.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Muted,
				TextAlignmentOptions.Center, false, true);
		}

		RectTransform lost = Rect("UI/Canvas/EpiloguePanel/Content/LostText");
		if (lost != null)
		{
			FromTop(lost, 616f, 1100f, 24f);
			SkinText(lost.GetComponent<TMP_Text>(), _font, UITheme.FsMicro, UITheme.Danger,
				TextAlignmentOptions.Center, false, true);
		}

		RectTransform restart = Rect("UI/Canvas/EpiloguePanel/Content/RestartButton");
		if (restart != null)
		{
			FromTop(restart, 680f, 320f, 64f);
			SkinImage(restart.GetComponent<Image>(), _btn, Color.white);
			SkinButton(restart.GetComponent<Button>(), _btn, _btnDown);

			Transform label = restart.Find("Text (TMP)");
			if (label != null)
			{
				Stretch((RectTransform)label, new Vector4(8f, 4f, 8f, 4f));
				SkinText(label.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Bg,
					TextAlignmentOptions.Center);
			}
		}

		Say("epilogue");
	}

	static void SkinEpilogueStat(string name, float x, Color color)
	{
		RectTransform block = Rect("UI/Canvas/EpiloguePanel/Content/StatsRow/StatBlock_" + name);
		if (block == null) return;

		Box(block, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(300f, 90f));

		RectTransform label = block.Find("Label") as RectTransform;
		if (label != null)
		{
			FromTop(label, 0f, 300f, 24f);
			SkinText(label.GetComponent<TMP_Text>(), _font, UITheme.FsMicro, UITheme.Muted,
				TextAlignmentOptions.Center);
		}

		RectTransform value = block.Find("Value") as RectTransform;
		if (value != null)
		{
			FromTop(value, 28f, 300f, 56f);
			SkinText(value.GetComponent<TMP_Text>(), _font, UITheme.FsName, color, TextAlignmentOptions.Center);
		}
	}

	static void ApplySealing(Scene scene)
	{
		RectTransform text = Rect("UI/Canvas/SealingPanel/SealingText");
		if (text != null)
		{
			Box(text, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(1100f, 140f));
			SkinText(text.GetComponent<TMP_Text>(), _fontBold, UITheme.FsName, UITheme.Bone,
				TextAlignmentOptions.Center, true);
		}

		RectTransform button = Rect("UI/Canvas/SealingPanel/SealButton");
		if (button != null)
		{
			Box(button, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(340f, 72f));
			SkinImage(button.GetComponent<Image>(), _btn, Color.white);
			SkinButton(button.GetComponent<Button>(), _btn, _btnDown);

			Transform label = button.Find("Text (TMP)");
			if (label != null)
			{
				Stretch((RectTransform)label, new Vector4(8f, 4f, 8f, 4f));
				SkinText(label.GetComponent<TMP_Text>(), _font, UITheme.FsBody, UITheme.Bg,
					TextAlignmentOptions.Center);
			}
		}

		Say("sealing panel");
	}

	static void ApplyFeed(Scene scene)
	{
		RectTransform feed = Rect("UI/Canvas/SynergyFeed");
		if (feed == null) return;

		// The toast sits under the dialogs: a synergy announced while an event is
		// open must not float above the dimmed screen.
		RectTransform gameScreen = Rect("UI/Canvas/GameScreen");
		if (gameScreen != null)
			feed.SetSiblingIndex(gameScreen.GetSiblingIndex() + 1);

		Box(feed, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(620f, 88f));

		Transform background = feed.Find("Background");
		if (background != null)
		{
			background.SetSiblingIndex(0);
			Stretch((RectTransform)background, Vector4.zero);
			SkinImage(background.GetComponent<Image>(), _panel, Color.white);
		}

		Transform line = feed.Find("Line");
		if (line != null)
		{
			Stretch((RectTransform)line, new Vector4(16f, 12f, 16f, 12f));
			var text = line.GetComponent<TMP_Text>();
			SkinText(text, _font, UITheme.FsMicro, UITheme.Bone, TextAlignmentOptions.Center, true);
			// Authoring placeholder must not be part of the saved scene.
			if (text != null) text.text = string.Empty;
		}

		// Swim upstream: the view clears this in Awake, but the edit-mode capture
		// and the scene view would otherwise show an empty toast over the cards.
		var group = feed.GetComponent<CanvasGroup>();
		if (group == null) group = feed.gameObject.AddComponent<CanvasGroup>();
		group.alpha = 0f;

		Say("synergy feed");
	}

	static void ApplyFader(Scene scene)
	{
		RectTransform fader = Rect("UI/Canvas/Fader");
		if (fader == null) return;

		Stretch(fader, Vector4.zero);
		fader.SetAsLastSibling();

		var img = fader.GetComponent<Image>();
		if (img != null)
		{
			img.sprite = null;
			img.type = Image.Type.Simple;
			img.color = new Color(0f, 0f, 0f, 1f);
			// The fader covers the whole screen; it must never swallow a click.
			img.raycastTarget = false;
		}
	}

	// ---------------------------------------------------------------- title

	static void ApplyTitle(Scene scene)
	{
		Transform canvas = Find("UI/Canvas");
		if (canvas == null) return;

		GameObject panel = Existing(canvas, "TitlePanel");
		var rt = (RectTransform)panel.transform;
		Stretch(rt, Vector4.zero);

		var backdrop = panel.GetComponent<Image>();
		if (backdrop == null) backdrop = panel.AddComponent<Image>();
		backdrop.sprite = null;
		backdrop.type = Image.Type.Simple;
		backdrop.color = UITheme.Bg;
		backdrop.raycastTarget = true;

		// keep it under the fader, above the game screen
		Transform fader = canvas.Find("Fader");
		if (fader != null)
		{
			panel.transform.SetSiblingIndex(fader.GetSiblingIndex());
			fader.SetAsLastSibling();
		}

		GameObject stars = Existing(panel.transform, "Stars");
		Stretch((RectTransform)stars.transform, Vector4.zero);
		var starImage = stars.GetComponent<Image>();
		if (starImage == null) starImage = stars.AddComponent<Image>();
		starImage.sprite = _stars;
		starImage.type = Image.Type.Tiled;
		starImage.pixelsPerUnitMultiplier = 0.25f;
		starImage.color = new Color(1f, 1f, 1f, 0.85f);
		starImage.raycastTarget = false;

		GameObject rocket = Existing(panel.transform, "Rocket");
		Box((RectTransform)rocket.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
			new Vector2(0f, 428f), new Vector2(128f, 128f));
		var rocketImage = rocket.GetComponent<Image>();
		if (rocketImage == null) rocketImage = rocket.AddComponent<Image>();
		SkinImage(rocketImage, Icon("title"), Color.white, Image.Type.Simple);
		rocketImage.preserveAspect = true;
		rocketImage.raycastTarget = false;

		TitleLabel(panel.transform, "Kicker", 320f, 1100f, 24f, _font, UITheme.FsMicro, UITheme.Muted,
			"TINY LOOP JAM 2026");

		TitleLabel(panel.transform, "Title", 236f, 1200f, 120f, _fontBold, UITheme.FsDisplay, UITheme.Bone,
			"THE LAST ARK");

		GameObject divider = Existing(panel.transform, "Divider");
		Box((RectTransform)divider.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
			new Vector2(0f, 168f), new Vector2(640f, 4f));
		var dividerImage = divider.GetComponent<Image>();
		if (dividerImage == null) dividerImage = divider.AddComponent<Image>();
		dividerImage.sprite = _divider;
		dividerImage.type = Image.Type.Simple;
		dividerImage.pixelsPerUnitMultiplier = 0.25f;
		dividerImage.color = UITheme.Line;
		dividerImage.raycastTarget = false;

		TitleLabel(panel.transform, "Sub", 122f, 1200f, 48f, _prose, UITheme.FsProse, UITheme.Muted,
			"Six seats. One engine. No return.");

		TitleLabel(panel.transform, "Rules", 20f, 900f, 110f, _font, UITheme.FsBody, UITheme.Bone,
			"PICK ONE SURVIVOR EACH TURN\nSIX SEATS IN THE ARK\nANY STAT AT ZERO ENDS THE RUN", true);

		// ---- start button
		GameObject buttonGo = Existing(panel.transform, "StartButton");
		var buttonRt = (RectTransform)buttonGo.transform;
		Box(buttonRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(360f, 72f));

		var buttonImage = buttonGo.GetComponent<Image>();
		if (buttonImage == null) buttonImage = buttonGo.AddComponent<Image>();
		SkinImage(buttonImage, _btn, Color.white);

		var button = buttonGo.GetComponent<Button>();
		if (button == null) button = buttonGo.AddComponent<Button>();
		button.targetGraphic = buttonImage;
		SkinButton(button, _btn, _btnDown);

		GameObject label = Existing(buttonGo.transform, "Text (TMP)");
		Stretch((RectTransform)label.transform, new Vector4(8f, 4f, 8f, 4f));
		var labelText = label.GetComponent<TMP_Text>();
		if (labelText == null) labelText = label.AddComponent<TextMeshProUGUI>();
		SkinText(labelText, _fontBold, UITheme.FsBody, UITheme.Bg, TextAlignmentOptions.Center);
		labelText.text = "BOARD THE ARK";

		TitleLabel(panel.transform, "Foot", -300f, 1400f, 24f, _font, UITheme.FsMicro, UITheme.Muted,
			"ART · KENNEY PIXEL UI (CC0) · GAME-ICONS.NET (CC BY 3.0)   FONTS · SILKSCREEN, VT323 (OFL)");

		// ---- wire it up
		var view = panel.GetComponent<TitleView>();
		if (view == null) view = panel.AddComponent<TitleView>();

		var flow = Get<GameFlow>("System/Game");
		if (flow != null)
		{
			var flowSo = new SerializedObject(flow);
			flowSo.FindProperty("titlePanel").objectReferenceValue = panel;
			flowSo.ApplyModifiedPropertiesWithoutUndo();
		}

		var viewSo = new SerializedObject(view);
		viewSo.FindProperty("flow").objectReferenceValue = flow;
		viewSo.FindProperty("panel").objectReferenceValue = panel;
		viewSo.FindProperty("startButton").objectReferenceValue = button;
		viewSo.ApplyModifiedPropertiesWithoutUndo();

		Say("title screen wired");
	}

	static void TitleLabel(Transform parent, string name, float y, float width, float height,
		TMP_FontAsset font, float size, Color color, string content, bool multiline = false)
	{
		GameObject go = Existing(parent, name);
		Box((RectTransform)go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
			new Vector2(0f, y), new Vector2(width, height));

		var text = go.GetComponent<TMP_Text>();
		if (text == null) text = go.AddComponent<TextMeshProUGUI>();

		SkinText(text, font, size, color, TextAlignmentOptions.Center, multiline);
		text.text = content;
	}
}
