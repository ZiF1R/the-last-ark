using UnityEditor;

/// <summary>
/// Head-less entry point for one full pass:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod ArkPipeline.RunFromBatch
/// 1. re-skins the scene and the card prefab,
/// 2. writes the frozen content seed into the database asset,
/// 3. renders review screenshots to Temp/Shots.
/// </summary>
public static class ArkPipeline
{
	public static void RunFromBatch()
	{
		PixelArtTheme.ApplyFromBatch();
		ContentSeedSync.SyncFromBatch();
		PixelArtCapture.CaptureFromBatch();
	}
}
