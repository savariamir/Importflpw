using ImportFlow.Events;

namespace ImportFlow.Domain;

public class EventNode<TEvent> where TEvent : ImportEvent
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public ImportState Status { set; get; }

    public State<TEvent> State { get; set; }
}