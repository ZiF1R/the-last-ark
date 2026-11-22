using System.Collections.Generic;

public class EventPicker
{
	public EventDef PickEvent(RunState state, ContentDatabase db)
	{
		var candidates = new List<EventDef>();
		int bestPriority = int.MinValue;

		foreach (EventDef e in db.events)
		{
			if (e == null) continue;
			if (e.once && state.usedEventIds.Contains(e.id)) continue;
			if (!e.Matches(state)) continue;

			if (e.priority > bestPriority)
			{
				bestPriority = e.priority;
				candidates.Clear();
				candidates.Add(e);
			}
			else if (e.priority == bestPriority)
			{
				candidates.Add(e);
			}
		}

		if (candidates.Count == 0)
			return null;

		return WeightedPicker.PickWeighted(candidates);
	}
}
