using ImportFlow.Domain.Repositories;
using ImportFlow.Events;

namespace ImportFlow.Infrastructure.InMemoryPersist;

public class InMemoryImportFlowRepository(
    IStateRepository<ImportEvent> repository
) : IImportFlowRepository
{
    private readonly List<Domain.ImportFlowProcess> _database = new();

    public async Task AddAsync(Domain.ImportFlowProcess import)
    {
        _database.Add(import);
        await repository.AddAsync(import.DownloadedFilesState);
    }

    public async Task<IEnumerable<Domain.ImportFlowProcess>> GatAllAsync()
    {
        foreach (var importFlowProcess in _database)
        {
            var states = await repository.GetAsync(importFlowProcess.Id);
            importFlowProcess.Set(states.ToList());
        }

        return _database.OrderByDescending(p => p.CreateAt);
    }

    public async Task<Domain.ImportFlowProcess> GatByIdAsync(Guid importFlowProcessId)
    {
        var first = _database.FirstOrDefault(p => p.Id == importFlowProcessId);
        if (first is null)
        {
            return null;
        }

        var states = await repository.GetAsync(first.Id);
        first.Set(states.ToList());
        return first;
    }
}