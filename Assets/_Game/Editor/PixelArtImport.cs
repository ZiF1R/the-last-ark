using UnityEditor;
using UnityEngine;

/// <summary>
/// Forces pixel-art friendly import settings on everything under Assets/_Game/Art.
/// 1 art pixel must stay 1 art pixel: Point filtering, no mipmaps, no compression.
/// Files tagged "_9s" get a 4px 9-slice border.
/// </summary>
public class PixelArtImport : AssetPostprocessor
{
	const string Root = "Assets/_Game/Art/";

	void OnPreprocessTexture()
	{
		if (!assetPath.StartsWith(Root)) return;

		var importer = (TextureImporter)assetImporter;

		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Single;
		importer.spritePixelsPerUnit = 100f;
		importer.filterMode = FilterMode.Point;
		importer.mipmapEnabled = false;
		importer.alphaIsTransparency = true;
		importer.wrapMode = TextureWrapMode.Clamp;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.maxTextureSize = 2048;
		importer.npotScale = TextureImporterNPOTScale.None;
		importer.sRGBTexture = true;
		importer.isReadable = false;

		var settings = new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		settings.spriteMeshType = SpriteMeshType.FullRect;

		// Files tagged "_9s" get a 4px 9-slice border so the frame stretches 4x.
		if (assetPath.Contains("_9s"))
			settings.spriteBorder = new Vector4(4f, 4f, 4f, 4f);

		importer.SetTextureSettings(settings);
	}
}
