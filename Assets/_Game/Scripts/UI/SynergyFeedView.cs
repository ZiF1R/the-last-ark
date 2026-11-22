using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SynergyFeedView : MonoBehaviour
{
	[SerializeField] TMP_Text line;
	[SerializeField] CanvasGroup group;
	Queue<(string text, bool bad)> _queue = new();
}
