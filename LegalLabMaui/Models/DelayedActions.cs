using Waher.Runtime.Timing;

namespace LegalLabMaui.Models;

public static class DelayedActions
{
    private static readonly Scheduler scheduler = new();

    public static void Add(IDelayedAction Action, DateTime When)
    {
        DateTime TP = Action.ScheduledFor;
        if (TP != DateTime.MinValue)
        {
            Action.ScheduledFor = DateTime.MinValue;
            scheduler.Remove(TP);
        }

        Action.ScheduledFor = scheduler.Add(When, ExecuteAction, Action);
    }

    public static void Remove(IDelayedAction Action)
    {
        if (Action.ScheduledFor != DateTime.MinValue)
        {
            Action.ScheduledFor = DateTime.MinValue;
            scheduler.Remove(Action.ScheduledFor);
        }
    }

    private static async Task ExecuteAction(object P)
    {
        IDelayedAction Action = (IDelayedAction)P;
        await Action.Action();
    }
}
