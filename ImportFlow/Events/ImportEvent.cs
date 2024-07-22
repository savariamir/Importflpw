namespace ImportFlow.Events;

public class ImportEvent
{
    protected ImportEvent()
    {
        CreatedAt = DateTime.Now;
        EventId = Guid.NewGuid();
    }

    public Guid EventId { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTime CreatedAt { get; }
    public int Number { set; get; }
    public Retry? Retry { set; get; }
}

public class Retry(string reason, string retryBy, Guid causationId)
{
    public Guid CausationId { private set; get; } = causationId;
    public string Reason { private set; get; } = reason;
    public string RetryBy {private set; get; } = retryBy;
}