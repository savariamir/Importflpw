namespace ImportFlow.Framework.Domain;

public class ImportFlow
{
    public Guid Id { get; private set; }

    public int PlatformId { get; private set; }

    public int? SupplierId { get; private set; }

    public DateTime CreateAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public State DownloadedFilesState { get; private set; }

    public IEnumerable<State>? InitialLoadState { get; private set; }

    public IEnumerable<State>? TransformationState { get; private set; }

    public IEnumerable<State>? DataExportState { get; private set; }
    

    private ImportFlow(ImportFlowProcessInfo info)
    {
        Id = info.CorrelationId;

        var downloadState = State.Create(
            StepsName.SupplierFiles,
            info.CorrelationId,
            info.CorrelationId,
            info.FilesCount);

        PlatformId = info.PlatformId;
        SupplierId = info.SupplierId;
        CreateAt = DateTime.Now;
        DownloadedFilesState = downloadState;
    }

    public static ImportFlow Start(ImportFlowProcessInfo info)
    {
        return new ImportFlow(info);
    }
    
    public void Set(List<State> states)
    {
        DownloadedFilesState = states.First(p=>p.Name == StepsName.SupplierFiles);
        InitialLoadState = states.Where(p=>p.Name == StepsName.InitialLoad);
        TransformationState = states.Where(p=>p.Name == StepsName.Transformation);
        DataExportState = states.Where(p=>p.Name == StepsName.DateExport);
    }
}