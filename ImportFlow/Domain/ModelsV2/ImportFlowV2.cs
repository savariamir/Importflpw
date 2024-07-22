namespace ImportFlow.Domain.ModelsV2;

public class ImportFlowV2
{
    public Guid Id { get; private set; }

    public int PlatformId { get; private set; }

    public int? SupplierId { get; private set; }

    public DateTime CreateAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public StateV2 DownloadedFilesState { get; private set; }

    public IEnumerable<StateV2>? InitialLoadState { get; private set; }

    public IEnumerable<StateV2>? TransformationState { get; private set; }

    public IEnumerable<StateV2>? DataExportState { get; private set; }
    

    private ImportFlowV2(ImportFlowProcessInfo info)
    {
        Id = info.CorrelationId;

        var downloadState = StateV2.Create(
            StepsName.SupplierFiles,
            info.CorrelationId,
            info.CorrelationId,
            info.FilesCount);

        PlatformId = info.PlatformId;
        SupplierId = info.SupplierId;
        CreateAt = DateTime.Now;
        DownloadedFilesState = downloadState;
    }

    public static ImportFlowV2 Start(ImportFlowProcessInfo info)
    {
        return new ImportFlowV2(info);
    }
    
    public void Set(List<StateV2> states)
    {
        DownloadedFilesState = states.First(p=>p.Name == StepsName.SupplierFiles);
        InitialLoadState = states.Where(p=>p.Name == StepsName.InitialLoad);
        TransformationState = states.Where(p=>p.Name == StepsName.Transformation);
        DataExportState = states.Where(p=>p.Name == StepsName.DateExport);
    }

    public void SetDownloadState(StateV2 state)
    {
        DownloadedFilesState = state;
    }

    public void SetInitialLoadStates(IEnumerable<StateV2>? state)
    {
        InitialLoadState = state;
    }

    public void SetTransformationStates(IEnumerable<StateV2>? state)
    {
        TransformationState = state;
    }

    public void SetDataExportStates(IEnumerable<StateV2>? state)
    {
        DataExportState = state;
    }
}