namespace ImportFlow.Framework.Domain;

public class ImportFlowProcessInfo
{
    public int PlatformId { get; set; }

    public int? SupplierId { get; set; }
    
    public int FilesCount { get; set; }
    
    public Guid CorrelationId { get; set; }
}