public class PendingEffect
{
	public int survival, science, morale;
	public int turnsLeft;
	public string sourceId;

	public PendingEffect(int s, int sc, int m, int turns, string source)
	{
		survival = s;
		science = sc;
		morale = m;
		turnsLeft = turns;
		sourceId = source;
	}

	public bool IsEmpty => survival == 0 && science == 0 && morale == 0;
}
