using ImportFlow.Events;

namespace ImportFlow.Domain.ModelsV2;

public class StateV2
{
    public Guid CorrelationId { get;private set; }

    public Guid CausationId { get;private set; }

    public string Name { get; private set; }

    public DateTime CreateAt { get; private set; }

    public IEnumerable<string> GetMessages()
    {
        return FailedEvents.Select(p => p.ErrorMessage)!;
    }

    public List<Message> FailedEvents { get; private set; } = new();

    public List<ImportEvent> Events { get; private set; } = new();
    
    public List<ImportEvent> SucceedEvents { get; private set; } = new();

    public long TotalCount { get; private set; }


    private StateV2(string name, Guid correlationId, Guid causationId, long totalCount)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
        Name = name;
        TotalCount = totalCount;
        CreateAt = DateTime.Now;
    }
    
    public static StateV2 Create(string name, Guid correlationId, Guid causationId, long totalCount)
    {
        return new StateV2(
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

            if (SucceedEvents.Count == 0 && FailedEvents.Count == TotalCount)
            {
                return ImportStatus.Failed;
            }

            var timeDifference = DateTime.Now - CreateAt;
            if (timeDifference.Minutes > 1)
            {
                return ImportStatus.PartiallyFailed;
                ;
            }

            return ImportStatus.Processing;
        }
    }
}