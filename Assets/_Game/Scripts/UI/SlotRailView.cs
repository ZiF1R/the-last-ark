using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotRailView : MonoBehaviour
{
	[SerializeField] Image[] slots;
	[SerializeField] Color emptyColor = new Color(0.13f, 0.20f, 0.23f);
	[SerializeField] Color filledColor = new Color(0.24f, 0.35f, 0.39f);
	[SerializeField] Color blockedColor = new Color(0.42f, 0.16f, 0.16f);

	[Header("Juice")]
	[SerializeField] float punchScale = 1.35f;
	[SerializeField] float punchDuration = 0.28f;

	Coroutine[] _punch;
	bool[] _filled;
	private bool[] _nowFilled;

	void Awake()
	{
		_punch = new Coroutine[slots.Length];
		_filled = new bool[slots.Length];
		_nowFilled = new bool[slots.Length];
	}

	void OnEnable()
	{
		GameEvents.RunStarted += Render;
		GameEvents.CrewChanged += Render;
	}

	void OnDisable()
	{
		GameEvents.RunStarted -= Render;
		GameEvents.CrewChanged -= Render;
	}

	public void ResetJuice()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (_punch[i] != null)
			{
				StopCoroutine(_punch[i]);
				_punch[i] = null;
			}

			slots[i].transform.localScale = Vector3.one;
			_filled[i] = false;
			_nowFilled[i] = false;
		}
	}

	void Render(RunState state)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].color = emptyColor;
			_nowFilled[i] = false;
		}

		int slotIndex = 0;

		foreach (CardDef card in state.crew)
		{
			if (card == null) continue;

			for (int k = 0; k < card.slotCost && slotIndex < slots.Length; k++)
			{
				while (slotIndex < slots.Length && state.sealedSlots.Contains(slotIndex))
					slotIndex++;

				if (slotIndex >= slots.Length) break;

				slots[slotIndex].color = filledColor;
				_nowFilled[slotIndex] = true;
				slotIndex++;
			}
		}

		foreach (int i in state.sealedSlots)
			if (i >= 0 && i < slots.Length)
				slots[i].color = blockedColor;

		for (int i = 0; i < slots.Length; i++)
		{
			if (_nowFilled[i] && !_filled[i])
			{
				if (_punch[i] != null) StopCoroutine(_punch[i]);

				_punch[i] = StartCoroutine(PunchRoutine(i));
			}

			_filled[i] = _nowFilled[i];
		}
	}

	IEnumerator PunchRoutine(int i)
	{
		Transform tr = slots[i].transform;
		Vector3 baseScale = tr.localScale;
		float t = 0f;

		while (t < punchDuration)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / punchDuration);
			tr.localScale = baseScale * (1f + (punchScale - 1f) * Mathf.Sin(k * Mathf.PI));
			yield return null;
		}

		tr.localScale = baseScale;
		_punch[i] = null;
	}
}
