namespace ImportFlow.QueryModels;

public class ImportFlowQuery
{
    public Guid ImportFlowProcessId { get;  set; }

    public int PlatformId { get;  set; }

    public string Platform { get; set; } = "Travelgate";

    public int? SupplierId { get;  set; }

    public DateTime CreateAt { get;  set; }

    public DateTime? UpdatedAt { get;  set; }
    
    public string Status { set; get; }

    public StateQuery State { get;  set; }
}

public class StateQuery
{
    public string Name { get; set; }
    
    public Guid CorrelationId { get; set; }

    public Guid CausationId { get; set; }
    
    public DateTime CreateAt { get;  set; }
    
    public IEnumerable<EventQuery> Events { get;  set; }
    
    public string Status { set; get; }

    public long TotalCount { get;  set; }

}

public class EventQuery
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get;  set; }
    
    public string Status { get;  set;}
    
    public StateQuery? State {get;  set;}
}