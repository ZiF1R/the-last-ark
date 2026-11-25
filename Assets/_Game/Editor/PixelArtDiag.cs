using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Temporary: explains why the card title does not draw.</summary>
public static class PixelArtDiag
{
	public static void Run()
	{
		var log = new StringBuilder();

		foreach (string file in new[] { "Silkscreen-Regular", "Silkscreen-Bold", "VT323-Regular" })
		{
			var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Game/Art/Fonts/" + file + " SDF.asset");
			if (f == null) { log.AppendLine(file + ": MISSING"); continue; }
			log.AppendLine($"{file}: mode={f.atlasPopulationMode} glyphs={f.characterTable?.Count} " +
				$"atlas={(f.atlasTextures != null && f.atlasTextures.Length > 0 && f.atlasTextures[0] != null ? f.atlasTextures[0].width + "x" + f.atlasTextures[0].height : "none")} " +
				$"src={(f.sourceFontFile != null ? f.sourceFontFile.name : "NULL")} " +
				$"mat={(f.material != null ? f.material.shader.name : "NULL")} " +
				$"weightTable={f.fontWeightTable?.Length} fallbacks={f.fallbackFontAssetTable?.Count}");
		}

		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/CardView.prefab");
		GameObject go = (GameObject)Object.Instantiate(prefab);

		Transform nameT = go.transform.Find("Name");
		var name = nameT.GetComponent<TMP_Text>();
		name.text = "HALLORAN";

		Transform roleT = go.transform.Find("Role");
		var role = roleT.GetComponent<TMP_Text>();
		role.text = "ENGINEER";

		Canvas.ForceUpdateCanvases();
		name.ForceMeshUpdate(true, true);
		role.ForceMeshUpdate(true, true);

		Describe(log, "Name", name);
		Describe(log, "Role", role);

		// A brand new object with the same font, as a control.
		var controlGo = new GameObject("~Control", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		var control = controlGo.GetComponent<TextMeshProUGUI>();
		control.font = name.font;
		control.fontSize = 48;
		control.text = "HALLORAN";
		control.ForceMeshUpdate(true, true);
		Describe(log, "Control", control);

		Object.DestroyImmediate(controlGo);
		Object.DestroyImmediate(go);

		Debug.Log("[Diag]\n" + log);
		System.IO.File.WriteAllText("Temp/ark_diag.txt", log.ToString());
	}

	static void Describe(StringBuilder log, string label, TMP_Text t)
	{
		var rt = (RectTransform)t.transform;
		log.AppendLine($"{label}: '{t.text}' font={(t.font != null ? t.font.name : "NULL")} " +
			$"size={t.fontSize} style={t.fontStyle} weight={t.fontWeight} " +
			$"glyphs={t.textInfo?.characterCount} lines={t.textInfo?.lineCount} " +
			$"verts={(t.textInfo != null && t.textInfo.meshInfo != null && t.textInfo.meshInfo.Length > 0 ? t.textInfo.meshInfo[0].vertexCount : -1)} " +
			$"pref={t.preferredWidth:F0}x{t.preferredHeight:F0} rect={rt.rect.width:F0}x{rt.rect.height:F0} " +
			$"pos={rt.anchoredPosition} scale={rt.localScale} color={t.color} enabled={t.enabled} " +
			$"active={t.gameObject.activeInHierarchy} canvasRenderer={(t.GetComponent<CanvasRenderer>() != null)}");
	}
}
