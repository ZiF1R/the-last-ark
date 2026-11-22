using System.Linq;

public static class TurnTick
{
	public static readonly int[] DriftTurns = { 2, 4, 6, 8, 10 };

	public static StatDelta Apply(RunState state)
	{
		StatDelta total = new(0, 0, 0);

		StatDelta passive = state.GetPassiveDelta();
		Add(total, state.ApplyStats(passive.survival, passive.science, passive.morale));
		Add(total, state.TickPendingEffects());

		if (DriftTurns.Contains(state.turn))
			Add(total, state.ApplyStats(-1, 0, 0));

		return total;
	}

	static void Add(StatDelta total, StatDelta delta)
	{
		total.survival += delta.survival;
		total.science += delta.science;
		total.morale += delta.morale;
	}

}
