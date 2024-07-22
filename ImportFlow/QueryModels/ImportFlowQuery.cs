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

    public IEnumerable<string> Messages { set; get; } = [];

    public StateQuery State { get;  set; }
    
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}