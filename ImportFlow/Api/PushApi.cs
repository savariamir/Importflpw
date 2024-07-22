using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.QueryModels;
using MassTransit;

namespace ImportFlow.Api;

public class PushApi(
    IImportFlowRepository repository,
    IStateRepository<SupplierFilesDownloaded> stateRepository,
    IBus bus)
{
    public async Task StartAsync()
    {
        var info = new ImportFlowProcessInfo
        {
            PlatformId = 1,
            SupplierId = 21,
            FilesCount = 4,
            CorrelationId = Guid.NewGuid(),
        };

        await StartAsync(info);
        //


        for (var i = 0; i < 4; i++)
        {
            var @event = new SupplierFilesDownloaded
            {
                CorrelationId = info.CorrelationId,
                CausationId = info.CorrelationId,
                Number = i + 1
            };
            // ...

            await stateRepository.PublishedAsync(@event);
            await bus.Publish(@event);
        }
    }

    public async Task<IEnumerable<ImportFlowProcess>> GetAsync()
    {
        return await repository.GatAllAsync();
    }

    public async Task<ImportFlowQuery> GatByIdAsync(Guid importFlowProcessId)
    {
        var import = await repository.GatByIdAsync(importFlowProcessId);
        var query = new ImportFlowQuery
        {
            ImportFlowProcessId = import.ImportFlowProcessId,
            PlatformId = import.PlatformId,
            SupplierId = import.SupplierId,
            CreateAt = import.CreateAt,
            UpdatedAt = import.UpdatedAt,
            Status = GetStatus(import),
            State = new StateQuery
            {
                Name = StepsName.SupplierFiles,
                CorrelationId = import.DownloadedFilesState.CorrelationId,
                CausationId = import.DownloadedFilesState.CausationId,
                CreateAt = import.DownloadedFilesState.CreateAt,
                // Status = GetStatus(import),
                TotalCount = import.DownloadedFilesState.TotalCount,
                Events = GetSupplierFilesEvents(import)
            }
        };

        // var visitor = new StatusVisitor();
        // visitor.Visit(query);
        return query;
    }

    private IEnumerable<EventQuery> GetSupplierFilesEvents(ImportFlowProcess import)
    {
        var events = new List<EventQuery>();
        foreach (var supplierFilesDownloaded in import.DownloadedFilesState.Events)
        {
            var eventQuery = new EventQuery
            {
                EventId = supplierFilesDownloaded.EventId,
                CreatedAt = supplierFilesDownloaded.CreatedAt,
                ErrorMessage = GetErrorMessageFrom(import.DownloadedFilesState.FailedEvents, supplierFilesDownloaded.EventId),
                State = CreateStateQueryModel(StepsName.Transformation, import.DownloadedFilesState.FailedEvents, supplierFilesDownloaded.EventId)
            };

            var initialLoadState =
                import.InitialLoadState?.FirstOrDefault(p => p.CausationId == supplierFilesDownloaded.EventId);
            if (initialLoadState is not null)
            {
                var initialLoadEvents = GetInitialLoadEvents(import, supplierFilesDownloaded);

                eventQuery.State = new StateQuery
                {
                    Name = StepsName.InitialLoad,
                    CorrelationId = initialLoadState.CorrelationId,
                    CausationId = initialLoadState.CausationId,
                    CreateAt = initialLoadState.CreateAt,
                    Status = initialLoadState.GetStatus().ToString(),
                    TotalCount = initialLoadState.TotalCount,
                    Events = initialLoadEvents
                };
            }

            events.Add(eventQuery);
        }

        return events;
    }

    private IEnumerable<EventQuery> GetInitialLoadEvents(ImportFlowProcess import,
        SupplierFilesDownloaded @event)
    {
        var events = new List<EventQuery>();

        var state = import.InitialLoadState?
            .FirstOrDefault(p => p.CausationId == @event.EventId);

        foreach (var initialLoadFinished in state.Events)
        {
            var eventQuery = new EventQuery
            {
                EventId = initialLoadFinished.EventId,
                CreatedAt = initialLoadFinished.CreatedAt,
                ErrorMessage = GetErrorMessageFrom(state.FailedEvents, initialLoadFinished.EventId),
                State = CreateStateQueryModel(StepsName.Transformation, state.FailedEvents, initialLoadFinished.EventId)
            };

            var transformationState = import.TransformationState?
                .FirstOrDefault(p => p.CausationId == initialLoadFinished.EventId);

            if (transformationState is not null)
            {
                var transformationEvents = GetTransformationEvents(import, initialLoadFinished);
                eventQuery.State = new StateQuery
                {
                    Name = StepsName.Transformation,
                    CorrelationId = transformationState.CorrelationId,
                    CausationId = transformationState.CausationId,
                    CreateAt = transformationState.CreateAt,
                    Status = transformationState.GetStatus().ToString(),
                    TotalCount = transformationState.TotalCount,
                    Events = transformationEvents
                };
            }

            events.Add(eventQuery);
        }

        return events;
    }

    private IEnumerable<EventQuery> GetTransformationEvents(ImportFlowProcess import,
        InitialLoadFinished @event)
    {
        var events = new List<EventQuery>();

        var state = import.TransformationState?
            .First(p => p.CausationId == @event.EventId);

        foreach (var transformationFinished in state.Events)
        {
            var eventQuery = new EventQuery
            {
                EventId = transformationFinished.EventId,
                CreatedAt = transformationFinished.CreatedAt,
                ErrorMessage = GetErrorMessageFrom(state.FailedEvents, transformationFinished.EventId),
                State = CreateStateQueryModel(StepsName.DateExport, state.FailedEvents, transformationFinished.EventId)
            };

            var dataExportState =
                import.DataExportState?.FirstOrDefault(p => p.CausationId == transformationFinished.EventId);
            if (dataExportState is not null)
            {
                var dataExportEvents = GetDataExportEvents(import, transformationFinished);

                eventQuery.State = new StateQuery
                {
                    Name = StepsName.DateExport,
                    CorrelationId = dataExportState.CorrelationId,
                    CausationId = dataExportState.CausationId,
                    CreateAt = dataExportState.CreateAt,
                    Status = dataExportState.GetStatus().ToString(),
                    TotalCount = dataExportState.TotalCount,
                    Events = dataExportEvents
                };
            }

            events.Add(eventQuery);
        }

        return events;
    }

    private IEnumerable<EventQuery> GetDataExportEvents(ImportFlowProcess import,
        TransformationFinished @event)
    {
        var events = new List<EventQuery>();
        var state = import.DataExportState!
            .First(p => p.CausationId == @event.EventId);

        foreach (var dataExported in state.Events)
        {
            var eventQuery = new EventQuery
            {
                EventId = dataExported.EventId,
                CreatedAt = dataExported.CreatedAt,
                ErrorMessage = GetErrorMessageFrom(state.FailedEvents, dataExported.EventId),
                State = CreateStateQueryModel(StepsName.DateExport, state.FailedEvents, dataExported.EventId),
            };
            
            events.Add(eventQuery);
        }

        return events;
    }
    
    private string? GetErrorMessageFrom(HashSet<Message> failedEvents, Guid eventId)
    {
        return failedEvents.FirstOrDefault(f => f.EventId == eventId)?.ErrorMessage;
    }

    private static StateQuery CreateStateQueryModel(string stepName, HashSet<Message> failedEvents, Guid eventId)
    {
        return new StateQuery
        {
            Name = stepName,
            Status = failedEvents
                .FirstOrDefault(f => f.EventId == eventId) != null
                ? ImportState.Failed.ToString()
                : ImportState.Processing.ToString()
        };
    }

    public async Task<IEnumerable<ImportFlowQuery>> GetImportFlowListAsync()
    {
        var imports = await repository.GatAllAsync();

        var result = new List<ImportFlowQuery>();
        foreach (var import in imports)
        {
            var query = new ImportFlowQuery
            {
                ImportFlowProcessId = import.ImportFlowProcessId,
                PlatformId = import.PlatformId,
                SupplierId = import.SupplierId,
                CreateAt = import.CreateAt,
                UpdatedAt = import.UpdatedAt,
                Status = GetStatus(import),
                // Messages = new []{ "Message 1", "Message 2"}
            };

            result.Add(query);
        }

        return result;
    }

    private string GetStatus(ImportFlowProcess import)
    {
        var timeDifference = DateTime.Now - import.CreateAt;
        if (timeDifference.Minutes > 1 && import.DataExportState?.Count() == 0)
        {
            return ImportState.Failed.ToString();
        }

        if (import.DataExportState?.Count() == 0)
        {
            return ImportState.Processing.ToString();
        }


        var isDownloadSucceed = import.DownloadedFilesState.GetStatus() == ImportState.Completed;


        var filesTotalCount = import.DownloadedFilesState.TotalCount;
        var isInitialLoadSucceed = import.InitialLoadState != null &&
                                   import.InitialLoadState.Count() == filesTotalCount &&
                                   import.InitialLoadState.All(p => p.GetStatus() == ImportState.Completed);

        var transformationCount = import.InitialLoadState?.Sum(p => p.TotalCount);
        var isTransformationSucceed = import.TransformationState != null &&
                                      import.TransformationState.Count() == transformationCount &&
                                      import.TransformationState.All(p => p.GetStatus() == ImportState.Completed);

        var dateExportCount = import.TransformationState?.Sum(p => p.TotalCount);
        var isDateExportSucceed = import.DataExportState != null &&
                                  import.DataExportState.Count() == dateExportCount &&
                                  import.DataExportState.All(p => p.GetStatus() == ImportState.Completed);


        var isSucceed = isDownloadSucceed && isInitialLoadSucceed && isTransformationSucceed && isDateExportSucceed;

        if (isSucceed)
        {
            return ImportState.Completed.ToString();
        }

        var anyDataExportCompleted = import.DataExportState?
            .Any(p => p.GetStatus() == ImportState.Completed) ?? false;

        if (!anyDataExportCompleted && timeDifference.Minutes > 1)
        {
            return ImportState.Failed.ToString();
        }

        if (anyDataExportCompleted && timeDifference.Minutes > 1)
        {
            return ImportState.PartiallyFailed.ToString();
        }

        return ImportState.Processing.ToString();
    }

    private async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = ImportFlowProcess.Start(info);
        await repository.AddAsync(importFlow);
    }
}