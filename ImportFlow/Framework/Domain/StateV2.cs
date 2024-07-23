using ImportFlow.Events;

namespace ImportFlow.Framework.Domain;

public class State
{
    public Guid CorrelationId { get; private set; }

    public Guid CausationId { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IEnumerable<string> GetMessages()
    {
        return FailedEvents.Select(p => p.ErrorMessage)!;
    }

    public List<Message> FailedEvents { get; private set; } = new();

    public List<ImportEvent> Events { get; private set; } = new();

    public List<ImportEvent> SucceedEvents { get; private set; } = new();

    public long TotalCount { get; private set; }


    private State(string name, Guid correlationId, Guid causationId, long totalCount)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
        Name = name;
        TotalCount = totalCount;
        CreatedAt = DateTime.Now;
    }

    public static State Create(string name, Guid correlationId, Guid causationId, long totalCount)
    {
        return new State(
            name,
            correlationId,
            causationId,
            totalCount);
    }

    public void Published(ImportEvent @event)
    {
        Events.Add(@event);
    }

    public void Finished(ImportEvent @event)
    {
        SucceedEvents.Add(@event);
    }

    public void Failed(ImportEvent @event, string errorMessage)
    {
        FailedEvents.Add(new Message(@event.EventId, errorMessage));
    }

    public string StatusName => Status.ToString();

    public ImportStatus Status
    {
        get
        {
            if (SucceedEvents.Count == TotalCount)
            {
                return ImportStatus.Completed;
            }

            var timeDifference = DateTime.Now - CreatedAt;

            if (SucceedEvents.Count == 0 && FailedEvents.Count == TotalCount && timeDifference.Seconds > 30)
            {
                return ImportStatus.Failed;
            }


            if (timeDifference.Minutes > 1)
            {
                return ImportStatus.PartialSuccess;
                ;
            }

            return ImportStatus.Processing;
        }
    }
}