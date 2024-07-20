using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Repositories;

public class ImportFlowRepository(
    IStateRepository<SupplierFilesDownloaded> supplierFilesRepository,
    IStateRepository<InitialLoadFinished> initialLoadRepository,
    IStateRepository<TransformationFinished> transformationRepository,
    IStateRepository<DataExported> dataExportRepository
) : IImportFlowRepository
{
    private readonly List<ImportFlowProcess> _database = new();

    public async Task AddAsync(ImportFlowProcess process)
    {
        _database.Add(process);
        await supplierFilesRepository.AddAsync(process.DownloadedFilesState);
    }

    public async Task<IEnumerable<ImportFlowProcess>> GatAllAsync()
    {
        foreach (var importFlowProcess in _database)
        {
            var supplierState = await supplierFilesRepository.GetAsync(importFlowProcess.ImportFlowProcessId);
            importFlowProcess.Set(supplierState.FirstOrDefault());
            importFlowProcess.Set(await initialLoadRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
            importFlowProcess.Set(await transformationRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
            importFlowProcess.Set(await dataExportRepository.GetAsync(importFlowProcess.ImportFlowProcessId));
        }

        return _database;
    }

    public async Task<ImportFlowProcess> GatByIdAsync(Guid importFlowProcessId)
    {
        var first = _database.FirstOrDefault(p => p.ImportFlowProcessId == importFlowProcessId);
        if (first is null)
        {
            return null;
        }
        
        var supplierState = await supplierFilesRepository.GetAsync(importFlowProcessId);
        first.Set(supplierState.First());
        first.Set(await initialLoadRepository.GetAsync(importFlowProcessId));
        first.Set(await transformationRepository.GetAsync(importFlowProcessId));
        first.Set(await dataExportRepository.GetAsync(importFlowProcessId));

        return first;
    }
}