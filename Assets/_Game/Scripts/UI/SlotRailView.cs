using UnityEngine;
using UnityEngine.UI;

public class SlotRailView : MonoBehaviour
{
	[SerializeField] Image[] slots;
	[SerializeField] Color emptyColor = new Color(0.13f, 0.20f, 0.23f);
	[SerializeField] Color filledColor = new Color(0.24f, 0.35f, 0.39f);

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

		int index = 0;
		foreach (CardDef card in state.crew)
			for (int k = 0; k < card.slotCost && index < slots.Length; k++)
				slots[index++].color = filledColor;
	}
}
