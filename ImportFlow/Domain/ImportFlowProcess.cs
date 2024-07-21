using ImportFlow.Events;
using ImportFlow.QueryModels;

namespace ImportFlow.Domain;

public class ImportFlowProcess: Import
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
    
    
    public State<SupplierFilesDownloaded> Tree { get; private set; }
    
    
    public void Accept(IImportFlowVisitor visitor)
    {
        visitor.Visit(this);
    }


    // public void BuildTree()
    // {
    //     Tree = DownloadedFilesState;
    //     // Link InitialLoadState with DownloadedFilesState
    //     foreach (var supplierFilesDownloaded in DownloadedFilesState.Events)
    //     {
    //         var initialLoadState = InitialLoadState?.FirstOrDefault(i => i.CausationId == supplierFilesDownloaded.EventId);
    //         if (initialLoadState != null)
    //         {
    //             supplierFilesDownloaded.InitialLoadState = initialLoadState;
    //
    //             // Link TransformationState with InitialLoadState
    //             foreach (var initEvent in initialLoadState.Events)
    //             {
    //                 var transformationState = TransformationState.Find(t => t.CausationId == initEvent.EventId);
    //                 if (transformationState != null)
    //                 {
    //                     initEvent.TransformationState = transformationState;
    //
    //                     // Link DataExportState with TransformationState
    //                     foreach (var transEvent in transformationState.Events)
    //                     {
    //                         var dataExportState = DataExportState.Find(d => d.CausationId == transEvent.EventId);
    //                         if (dataExportState != null)
    //                         {
    //                             transEvent.DataExportState = dataExportState;
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

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

    public override void BuildTree()
    {
        throw new NotImplementedException();
    }
}