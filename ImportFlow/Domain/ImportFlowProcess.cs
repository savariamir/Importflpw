using ImportFlow.Events;

namespace ImportFlow.Domain;

public class ImportFlowProcess
{
    public Guid ImportFlowProcessId { get; private set; }

    public int PlatformId { get; private set; }

    public int? SupplierId { get; private set; }

    public DateTime CreateAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public State<SupplierFilesDownloaded> DownloadedFilesState { get; private set; }

    public IEnumerable<State<InitialLoadFinished>>? InitialLoadState { get; private set; }

    public IEnumerable<State<TransformationFinished>>? TransformationState { get; private set; }

    public IEnumerable<State<DataExported>>? DataExportState { get; private set; }

    private ImportFlowProcess(ImportFlowProcessInfo info)
    {
        ImportFlowProcessId = info.CorrelationId;

        var downloadState = State<SupplierFilesDownloaded>.Create(
            StepsName.SupplierFiles,
            info.CorrelationId,
            info.CorrelationId,
            info.FilesCount);

        PlatformId = info.PlatformId;
        SupplierId = info.SupplierId;
        CreateAt = DateTime.Now;
        DownloadedFilesState = downloadState;
    }

    public static ImportFlowProcess Start(ImportFlowProcessInfo info)
    {
        return new ImportFlowProcess(info);
    }

    public void Set(State<SupplierFilesDownloaded> state)
    {
        DownloadedFilesState = state;
    }

    public void Set(IEnumerable<State<InitialLoadFinished>>? state)
    {
        InitialLoadState = state;
    }

    public void Set(IEnumerable<State<TransformationFinished>>? state)
    {
        TransformationState = state;
    }

    public void Set(IEnumerable<State<DataExported>>? state)
    {
        DataExportState = state;
    }
}