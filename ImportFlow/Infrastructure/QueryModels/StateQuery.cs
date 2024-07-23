namespace ImportFlow.Infrastructure.QueryModels;

public class StateQueryModel
{
    public string Name { get; set; }
    
    public Guid CorrelationId { get; set; }

    public Guid CausationId { get; set; }
    
    public DateTime CreateAt { get;  set; }
    
    public IEnumerable<EventQueryModel>? Events { get;  set; }
    public string Status { set; get; }

    public long TotalCount { get;  set; }
    
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

}