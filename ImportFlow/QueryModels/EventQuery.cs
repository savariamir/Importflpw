using ImportFlow.Domain;

namespace ImportFlow.QueryModels;

public class EventQuery
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public StateQuery? State { get; set; }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}