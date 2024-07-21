using ImportFlow.Events;
using ImportFlow.QueryModels;

namespace ImportFlow.Domain;

public class State<TEvent> : Import where TEvent : ImportEvent
{
    public Guid CorrelationId { get; set; }

    public Guid CausationId { get; set; }

    public string Name { get; private set; }

    public DateTime CreateAt { get; private set; }

    public IEnumerable<string> GetMessages()
    {
        return FailedEvents.Select(p => p.ErrorMessage)!;
    }

    public HashSet<Message> FailedEvents { get; private set; } = new();

    public HashSet<Guid> SucceedEvents { get; private set; } = new();

    public HashSet<TEvent> Events { get; private set; } = new();

    public IEnumerable<EventNode<TEvent>> EventsInfo { get; private set; }

    public long TotalCount { get; private set; }


    public void Accept(IImportFlowVisitor visitor)
    {
        visitor.Visit(this);
    }


    private State(string name, Guid correlationId, Guid causationId, int totalCount)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
        Name = name;
        TotalCount = totalCount;
        CreateAt = DateTime.Now;
    }

    public static State<TEvent> Create(string name, Guid correlationId, Guid causationId,
        int totalCount)
    {
        return new State<TEvent>(
            name,
            correlationId,
            causationId,
            totalCount);
    }

    public void Published(TEvent @event)
    {
        Events.Add(@event);
    }

    public void Finished(TEvent @event)
    {
        SucceedEvents.Add(@event.EventId);
    }

    public void Failed(TEvent @event, string errorMessage)
    {
        FailedEvents.Add(new Message(@event.EventId, errorMessage));
    }


    public ImportState Status
    {
        get
        {
            if (SucceedEvents.Count == TotalCount)
            {
                return ImportState.Completed;
            }

            if (SucceedEvents.Count == 0 && FailedEvents.Count == TotalCount)
            {
                return ImportState.Failed;
                ;
            }

            var timeDifference = DateTime.Now - CreateAt;
            if (timeDifference.Minutes > 1)
            {
                return ImportState.PartiallyFailed;
                ;
            }

            return ImportState.Processing;
            ;
        }
    }

    public override void BuildTree()
    {
        throw new NotImplementedException();
    }
}