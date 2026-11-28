using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Production WebGL build for the itch.io submission.
///   Unity.exe -batchmode -quit -projectPath . -executeMethod WebGLBuild.Configure
///   Unity.exe -batchmode -quit -projectPath . -executeMethod WebGLBuild.BuildFromBatch
/// Configure() is idempotent and safe to run every time.
/// </summary>
public static class WebGLBuild
{
	const string OutputDir = "Builds/WebGL";

	[MenuItem("Tools/The Last Ark/Configure WebGL Settings", false, 30)]
	public static void ConfigureMenuItem() => Configure();

	[MenuItem("Tools/The Last Ark/Build WebGL (itch.io)", false, 31)]
	public static void BuildMenuItem()
	{
		BuildFromBatch();
	}

	public static void Configure() => ApplySettings();

	public static void BuildFromBatch()
	{
		ApplySettings();

		BuildPlayerOptions options = new()
		{
			scenes = new[] { "Assets/Scenes/MainScene.unity" },
			locationPathName = OutputDir,
			target = BuildTarget.WebGL,
		};

		BuildReport report = BuildPipeline.BuildPlayer(options);

		if (report.summary.result != BuildResult.Succeeded)
		{
			Debug.LogError("[Build] failed: " + report.summary.result);
			return;
		}

		var log = new System.Text.StringBuilder();
		log.AppendLine($"[Build] {report.summary.result} -> {OutputDir}");
		log.AppendLine($"[Build] total {report.summary.totalSize / (1024 * 1024)} MB, " +
			$"time {report.summary.totalTime.TotalSeconds:0}s");

		foreach (FileInfo file in new DirectoryInfo(OutputDir).GetFiles("*", SearchOption.AllDirectories)
			.OrderByDescending(f => f.Length))
			log.AppendLine($"  {file.FullName.Substring(Path.GetFullPath(OutputDir).Length + 1),-28} " +
				$"{file.Length / 1024} kb");

		Debug.Log(log.ToString());
	}

	static void ApplySettings()
	{
		// itch.io serves the build without custom server headers, so the loader
		// cannot rely on gzip being transparently decompressed. Gzip plus the
		// embedded fallback decompressor is the combination that always works
		// there while still cutting the payload roughly 4x versus no
		// compression at all.
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
		PlayerSettings.WebGL.decompressionFallback = true;

		// Custom per-project template: the canvas fills the window instead of
		// being hard-locked to 1280x720, so itch's fullscreen button actually
		// stretches the game. Lives in Assets/WebGLTemplates/TheLastArk.
		PlayerSettings.WebGL.template = "APPLICATION:TheLastArk";

		// Data caching keeps the downloaded content in the browser's IndexedDB,
		// so a reload after the first one starts in a second or two.
		PlayerSettings.WebGL.dataCaching = true;

		// Explicitly-thrown exceptions only: keeps nullrefs and our own thrown
		// errors visible in the browser console while avoiding the multi-MB
		// cost of full stack traces. The project already ships with
		// webGLExceptionSupport = 1 (this mode), nothing to change here.

		// What the itch.io viewport should present at launch. 1280x720 is the
		// reference resolution the whole UI is balanced against.
		PlayerSettings.defaultWebScreenWidth = 1280;
		PlayerSettings.defaultWebScreenHeight = 720;

		// IL2CPP managed stripping: removes unused engine/library code from the
		// WebAssembly. Medium is safe for this project (UI + TMP, no reflection
		// tricks) and noticeably smaller than the default.
		PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Medium);

		// PlayerSettings writes through on its own; no explicit save needed.
		Debug.Log("[Build] WebGL settings applied: gzip+fallback, minimal template, " +
			"1280x720, data caching, stripping Medium");
	}
}
