using ImportFlow.Domain;

namespace ImportFlow.QueryModels;

public class EventQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }
    public IEnumerable<MessageQueryModel>? FailedEvents { set; get; }
    public StateQueryModel? State { get; set; }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class MessageQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get;  set; }
}