namespace ImportFlow.Domain;

public class StateOptions
{
    public string Name { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public int TotalCount { get; set; }
    public bool HasEvents { get; set; }
}