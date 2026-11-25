using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotRailView : MonoBehaviour
{
	[SerializeField] Image[] slots;
	[SerializeField] TMP_Text[] slotLabels;

	[Header("Colors")]
	[SerializeField] Color emptyColor = new Color(0.07f, 0.13f, 0.16f);
	[SerializeField] Color sealedColor = new Color(0.42f, 0.16f, 0.16f);
	[SerializeField] float roleTint = 0.35f;

	[Header("Juice")]
	[SerializeField] float flashDuration = 0.28f;

	Coroutine[] _flash;
	bool[] _filled;
	bool[] _nowFilled;

	void Awake()
	{
		_flash = new Coroutine[slots.Length];
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
			if (_flash[i] != null)
			{
				StopCoroutine(_flash[i]);
				_flash[i] = null;
			}

			slots[i].color = emptyColor;
			_filled[i] = false;
			_nowFilled[i] = false;

			if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
				slotLabels[i].text = "";
		}
	}

	void Render(RunState state)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			_nowFilled[i] = false;
			SetSlot(i, emptyColor, UITheme.Muted, "");
		}

		int slotIndex = 0;

		foreach (CardDef card in state.crew)
		{
			if (card == null) continue;

			Color tint = Color.Lerp(emptyColor, UITheme.RoleColor(card.role), roleTint);
			Color labelColor = Color.Lerp(UITheme.RoleColor(card.role), UITheme.Bone, 0.45f);
			string name = card.displayName.ToUpperInvariant();

			for (int k = 0; k < card.slotCost && slotIndex < slots.Length; k++)
			{
				while (slotIndex < slots.Length && state.sealedSlots.Contains(slotIndex))
					slotIndex++;

				if (slotIndex >= slots.Length) break;

				SetSlot(slotIndex, tint, labelColor, k == 0 ? name : "");
				_nowFilled[slotIndex] = true;
				slotIndex++;
			}
		}

		foreach (int i in state.sealedSlots)
		{
			if (i >= 0 && i < slots.Length)
				SetSlot(i, sealedColor, UITheme.Danger, "SEALED");
		}

		for (int i = 0; i < slots.Length; i++)
		{
			if (_nowFilled[i] && !_filled[i])
			{
				if (_flash[i] != null) StopCoroutine(_flash[i]);
				_flash[i] = StartCoroutine(FlashRoutine(i, slots[i].color));
			}

			_filled[i] = _nowFilled[i];
		}
	}

	void SetSlot(int index, Color frame, Color labelColor, string text)
	{
		slots[index].color = frame;

		if (slotLabels == null || index >= slotLabels.Length || slotLabels[index] == null) return;

		slotLabels[index].text = text;
		slotLabels[index].color = labelColor;
	}

	IEnumerator FlashRoutine(int index, Color target)
	{
		Color from = Color.Lerp(target, UITheme.Bone, 0.55f);
		float t = 0f;

		slots[index].color = from;

		while (t < flashDuration)
		{
			t += Time.unscaledDeltaTime;
			slots[index].color = Color.Lerp(from, target, Mathf.Clamp01(t / flashDuration));
			yield return null;
		}

		slots[index].color = target;
		_flash[index] = null;
	}
}
