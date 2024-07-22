using ImportFlow.Events;

namespace ImportFlow.Domain;

public class EventNode<TEvent> where TEvent : ImportEvent
{
    public EventNode(TEvent @event)
    {
        Event = @event;
    }

    public TEvent Event { get; set; }

    public string? ErrorMessage { get; set; }

    public ImportState? Status { set; get; }

    public State<TEvent>? State { get; set; }
    
    public void Accept(IImportFlowVisitor visitor)
    {
        visitor.Visit(this);
    }
}