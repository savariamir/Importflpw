namespace ImportFlow.Domain;

public class ImportProcessOptions
{
    public int PlatformId { get; set; }

    public int? SupplierId { get; set; }
    public Guid CorrelationId { get; set; }
    public required Dictionary<string, string> Transitions { set; get; }
}