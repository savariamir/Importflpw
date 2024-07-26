namespace ImportFlow.Domain;

public class ImportProcess
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
    

    private ImportProcess(ImportProcessOptions options)
    {
        Id = options.CorrelationId;

        var downloadState = State.Start(
            options.StepName,
            options.CorrelationId,
            options.CorrelationId,
            options.TotalEventsCount);

        PlatformId = options.PlatformId;
        SupplierId = options.SupplierId;
        CreateAt = DateTime.Now;
        DownloadedFilesState = downloadState;
    }

    public static ImportProcess Start(ImportProcessOptions options)
    {
        return new ImportProcess(options);
    }
    
    public void Set(List<State> states)
    {
        DownloadedFilesState = states.First(p=>p.Name == StepsName.PushApi);
        InitialLoadState = states.Where(p=>p.Name == StepsName.InitialLoad);
        TransformationState = states.Where(p=>p.Name == StepsName.Transformation);
        DataExportState = states.Where(p=>p.Name == StepsName.DateExport);
    }
}