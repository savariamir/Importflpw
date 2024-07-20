namespace ImportFlow;

public class SupplierFilesInfo
{
    public Guid CorrelationId { get; set; }

    public Guid CausationId { get; set; }

    public int TotalCount { get; set; }

    public int PlatformId { get; set; }

    public int? SupplierId { get; set; }
}