namespace ImportFlow.Framework.QueryModels;

public class EventQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public RetryQueryModel? Retry { set; get; }

    public string EventName { get; set; }
    public IEnumerable<MessageQueryModel>? FailedEvents { set; get; }
    public StateQueryModel? State { get; set; }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class RetryQueryModel
{
    public Guid? CausationId { set; get; }
    public string? Reason {  set; get; }
    public string? RetryBy { set; get; }
}

public class MessageQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get;  set; }
}