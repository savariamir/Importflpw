using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;

namespace ImportFlow.Repositories.V2;

public class ImportFlowRepositoryV2 (
    IStateRepositoryV2<SupplierFilesDownloaded> downloadStepRepository,
    IStateRepositoryV2<InitialLoadFinished> initialStepRepository,
    IStateRepositoryV2<TransformationFinished> transformationStepRepository,
    IStateRepositoryV2<DataExported> dataExportStepRepository
    
    
    ) : IImportFlowRepositoryV2
{
    private readonly List<ImportFlowV2> _database = new();
    public async Task AddAsync(ImportFlowV2 import)
    {
        _database.Add(import);
        await downloadStepRepository.AddAsync(import.DownloadedFilesState);
    }

    public async Task<IEnumerable<ImportFlowV2>> GatAllAsync()
    {
        foreach (var importFlowProcess in _database)
        {
            var supplierState = await downloadStepRepository.GetAsync(importFlowProcess.ImportFlowProcessId);
            importFlowProcess.SetDownloadState(supplierState.First());
            importFlowProcess.SetInitialLoadStates(await initialStepRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
            importFlowProcess.SetTransformationStates(await transformationStepRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
            importFlowProcess.SetDataExportStates(await dataExportStepRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
        }

        return _database;
    }

    public async Task<ImportFlowV2> GatByIdAsync(Guid importFlowProcessId)
    {
        var first = _database.FirstOrDefault(p => p.ImportFlowProcessId == importFlowProcessId);
        if (first is null)
        {
            return null;
        }
        
        var supplierState = await downloadStepRepository.GetAsync(importFlowProcessId);
        first.SetDownloadState(supplierState.First());
        first.SetInitialLoadStates(await initialStepRepository.GetAsync(importFlowProcessId));
        first.SetTransformationStates(await transformationStepRepository.GetAsync(importFlowProcessId));
        first.SetDataExportStates(await dataExportStepRepository.GetAsync(importFlowProcessId));

        return first;
    }
}