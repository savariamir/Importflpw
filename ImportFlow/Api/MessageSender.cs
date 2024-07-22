using ImportFlow.Domain.Repositories;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Api;

public class MessageSender( IImportFlowRepositoryV2 repository,
    IStateRepositoryV2<SupplierFilesDownloaded> stateRepository,
    IBus bus)
{
    public async Task ResendAsync(string stepName, Guid EventId)
    {
        
    }
}