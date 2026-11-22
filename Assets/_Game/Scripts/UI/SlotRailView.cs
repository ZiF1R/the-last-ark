using UnityEngine;
using UnityEngine.UI;

public class SlotRailView : MonoBehaviour
{
	[SerializeField] Image[] slots;
	[SerializeField] Color emptyColor = new Color(0.13f, 0.20f, 0.23f);
	[SerializeField] Color filledColor = new Color(0.24f, 0.35f, 0.39f);
	[SerializeField] Color blockedColor = new Color(0.42f, 0.16f, 0.16f);

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

	void Render(RunState state)
	{
		for (int i = 0; i < slots.Length; i++)
			slots[i].color = emptyColor;

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
				slotIndex++;
			}
		}

		foreach (int i in state.sealedSlots)
			if (i >= 0 && i < slots.Length)
				slots[i].color = blockedColor;
	}
}
