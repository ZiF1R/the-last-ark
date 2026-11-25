using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Copies the frozen ContentSeed lists into the ContentDatabase asset that the
/// scene actually points at.
///
/// The old GameFlow "Import Seed" context menu only assigned the lists in memory:
/// the asset on disk (and therefore any later build made without that menu click)
/// still held the 7-card placeholder set. This tool writes the asset down.
/// </summary>
public static class ContentSeedSync
{
	const string ScenePath = "Assets/Scenes/MainScene.unity";

	[MenuItem("Tools/The Last Ark/Sync Content From Seed", false, 20)]
	public static void SyncMenuItem() => Sync();

	public static void SyncFromBatch() => Sync();

	static void Sync()
	{
		EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

		GameFlow flow = Object.FindAnyObjectByType<GameFlow>();
		if (flow == null || flow.db == null)
		{
			Debug.LogError("[Seed] GameFlow or its ContentDatabase is missing");
			return;
		}

		flow.db.cards = ContentSeed.Cards;
		flow.db.events = ContentSeed.Events;
		flow.db.endings = ContentSeed.Endings;
		flow.db.synergies = ContentSeed.Synergies;

		EditorUtility.SetDirty(flow.db);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"[Seed] cards={flow.db.cards.Count} events={flow.db.events.Count} " +
			$"endings={flow.db.endings.Count} synergies={flow.db.synergies.Count}");
	}
}
