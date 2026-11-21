using UnityEngine;

public class DevTools : MonoBehaviour
{
	[SerializeField] ContentDatabase db;

	[ContextMenu("Debug/Tests — All")]
	public void RunAll()
	{
		var (p1, f1) = DraftTests.Run(db);
		// var (p2, f2) = EventTests.Run(db);

		Debug.Log($"<b>[Tests] PASS {p1} / FAIL {f1}</b>");
	}

	[ContextMenu("Debug/Tests — Draft")]
	public void RunDraftTests()
	{
		var (passed, failed) = DraftTests.Run(db);
		Debug.Log($"<b>[Draft] PASS {passed} / FAIL {failed}</b>");
	}

	[ContextMenu("Debug/Simulate 1000 runs")]
	public void Simulate() => RunSimulator.Run(db, 1000);

	[ContextMenu("Debug/Reset GameEvents")]
	public void ResetEvents() => GameEvents.ResetAll();
}
