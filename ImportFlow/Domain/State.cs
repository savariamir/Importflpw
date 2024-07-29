using ImportFlow.Events;

namespace ImportFlow.Domain;

public class State
{
    public Guid CorrelationId { get; private set; }

    public Guid CausationId { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedAt { get; }

    public List<FailedEvent> FailedEvents { get; } = new();

    public List<ImportEvent> Events { get; } = new();

    public List<ImportEvent> SucceedEvents { get; } = new();

    public bool IsInitialState { get; private set; }

    public long TotalCount { get; }
    
    public bool HasEvents { get; private set; }

    public bool IsFinished { get; private set; }


    private State(StateOptions options, bool isInitialState)
    {
        CorrelationId = options.CorrelationId;
        CausationId = options.CausationId;
        Name = options.Name;
        TotalCount = options.TotalCount;
        CreatedAt = DateTime.Now;
        IsInitialState = isInitialState;
        HasEvents = options.HaveEvents;
    }

    public static State InitiateState(StateOptions options)
    {
        return new State(options, true);
    }

    public static State AddState(StateOptions options)
    {
        return new State(options, false);
    }

    public void Finish()
    {
        IsFinished = true;
    }

    public void Add(ImportEvent @event)
    {
        Events.Add(@event);
    }

    public void Succeed(ImportEvent @event)
    {
        SucceedEvents.Add(@event);
    }

    public void Fail(ImportEvent @event, string errorMessage)
    {
        FailedEvents.Add(new FailedEvent(@event.EventId, errorMessage));
    }


    public ImportStatus Status
    {
        get
        {
            if (IsFinished)
            {
                return ImportStatus.Completed;
            }

            if (SucceedEvents.Count == TotalCount)
            {
                return ImportStatus.Completed;
            }

            var timeDifference = DateTime.Now - CreatedAt;

            if (SucceedEvents.Count == 0 && FailedEvents.Count == TotalCount && timeDifference.Minutes > 1)
            {
                return ImportStatus.Failed;
            }


            if (timeDifference.Minutes > 1)
            {
                return ImportStatus.PartialSuccess;
            }

            return ImportStatus.Processing;
        }
    }
}