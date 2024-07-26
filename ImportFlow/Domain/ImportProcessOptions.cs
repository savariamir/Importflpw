namespace ImportFlow.Domain;

public class ImportProcessOptions
{
    public string StepName { get; set; }
    public int PlatformId { get; set; }

    public int? SupplierId { get; set; }
    
    public int TotalEventsCount { get; set; }
    
    public Guid CorrelationId { get; set; }
}