namespace LegalLabMaui.Models;

public interface IDelayedAction
{
    Task Action();
    bool Changed { get; }
    DateTime ScheduledFor { get; set; }
}
