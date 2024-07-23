namespace ImportFlow.Infrastructure.QueryModels;

public class EventQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public RetryQueryModel? Retry { set; get; }

    public string EventName { get; set; }
    public IEnumerable<FailedEventQueryModel>? FailedEvents { set; get; }
    public StateQueryModel? State { get; set; }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}