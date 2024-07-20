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
                Status = "",
                TotalCount = import.DownloadedFilesState.TotalCount,
                Events = GetSupplierFilesEvents(import)
            }
        };

        return query;

        var filesEventsQuery = import.DownloadedFilesState.Events.Select(supplierFilesDownloaded =>
            new EventQuery
            {
                EventId = supplierFilesDownloaded.EventId,
                CreatedAt = supplierFilesDownloaded.CreateAt,
                ErrorMessage = import.DownloadedFilesState.FailedEvents
                    .FirstOrDefault(f => f.EventId == supplierFilesDownloaded.EventId)
                    ?.ErrorMessage,
                Status = ""
            });


        foreach (var supplierFilesDownloaded in import.DownloadedFilesState.Events)
        {
            var initialLoadState = import.InitialLoadState?
                .FirstOrDefault(p => p.CausationId == supplierFilesDownloaded.EventId);
            if (initialLoadState is null) continue;

            var supplierFilesDownloadedEvent =
                filesEventsQuery.First(p => p.EventId == supplierFilesDownloaded.EventId);

            var initialStateQuery = new StateQuery
            {
                Name = StepsName.InitialLoad,
                CorrelationId = initialLoadState.CorrelationId,
                CausationId = initialLoadState.CausationId,
                CreateAt = initialLoadState.CreateAt,
                Status = "",
                TotalCount = initialLoadState.TotalCount,
            };

            var initialEventsQuery = initialLoadState.Events.Select(initialLoadFinished => new EventQuery
            {
                EventId = initialLoadFinished.EventId,
                CreatedAt = initialLoadFinished.CreateAt,
                ErrorMessage = initialLoadState.FailedEvents
                    .FirstOrDefault(f => f.EventId == initialLoadFinished.EventId)
                    ?.ErrorMessage,
                Status = ""
            });

            foreach (var initialLoadFinished in initialLoadState.Events)
            {
                var transformationState = import.TransformationState?
                    .FirstOrDefault(p => p.CausationId == initialLoadFinished.EventId);
                if (transformationState is null) continue;

                var initialLoadEvents = initialEventsQuery.First(p => p.EventId == supplierFilesDownloaded.EventId);

                var transformationStateQuery = new StateQuery
                {
                    Name = StepsName.InitialLoad,
                    CorrelationId = transformationState.CorrelationId,
                    CausationId = transformationState.CausationId,
                    CreateAt = transformationState.CreateAt,
                    Status = "",
                    TotalCount = transformationState.TotalCount,
                };

                var transformationEventsQuery = transformationState.Events.Select(tra => new EventQuery
                {
                    EventId = tra.EventId,
                    CreatedAt = tra.CreateAt,
                    ErrorMessage = transformationState.FailedEvents
                        .FirstOrDefault(f => f.EventId == tra.EventId)
                        ?.ErrorMessage,
                    Status = "",
                });

                foreach (var transformationFinished in transformationState.Events)
                {
                    var dartExportState = import.DataExportState?
                        .FirstOrDefault(p => p.CausationId == transformationFinished.EventId);
                    if (dartExportState is null) continue;


                    var transformationEvensts =
                        transformationEventsQuery.First(p => p.EventId == transformationFinished.EventId);

                    var dartExportStateQuery = new StateQuery
                    {
                        Name = StepsName.InitialLoad,
                        CorrelationId = dartExportState.CorrelationId,
                        CausationId = dartExportState.CausationId,
                        CreateAt = dartExportState.CreateAt,
                        Status = "",
                        TotalCount = dartExportState.TotalCount,
                    };

                    var dartExportEvents = dartExportState.Events.Select(tra => new EventQuery
                    {
                        EventId = tra.EventId,
                        CreatedAt = tra.CreateAt,
                        ErrorMessage = dartExportState.FailedEvents
                            .FirstOrDefault(f => f.EventId == tra.EventId)
                            ?.ErrorMessage,
                        Status = ""
                    });

                    dartExportStateQuery.Events = dartExportEvents;

                    transformationEvensts.State = dartExportStateQuery;
                }

                initialLoadEvents.State = transformationStateQuery;
            }

            supplierFilesDownloadedEvent.State = initialStateQuery;
        }


        query.State.Events = filesEventsQuery;

        return query;
    }


    private IEnumerable<EventQuery> GetSupplierFilesEvents(ImportFlowProcess import)
    {
        var events = new List<EventQuery>();
        foreach (var supplierFilesDownloaded in import.DownloadedFilesState.Events)
        {
            var eventQuery =new EventQuery
            {
                EventId = supplierFilesDownloaded.EventId,
                CreatedAt = supplierFilesDownloaded.CreateAt,
                ErrorMessage = import.DownloadedFilesState.FailedEvents
                    .FirstOrDefault(f => f.EventId == supplierFilesDownloaded.EventId)
                    ?.ErrorMessage,
                Status = ""
            };
            
            var initialLoadState = import.InitialLoadState?
                .FirstOrDefault(p => p.CausationId == supplierFilesDownloaded.EventId);
            if (initialLoadState is null) return Enumerable.Empty<EventQuery>();
            
            var initialLoadEvents = GetInitialLoadEvents(import, supplierFilesDownloaded);

            eventQuery.State = new StateQuery
            {
                Name = StepsName.InitialLoad,
                CorrelationId = initialLoadState.CorrelationId,
                CausationId = initialLoadState.CausationId,
                CreateAt = initialLoadState.CreateAt,
                Status = "",
                TotalCount = initialLoadState.TotalCount,
                Events = initialLoadEvents
            };
            
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
        if (state is null)
        {
            return events;
        };
        
        foreach (var initialLoadFinished in state.Events)
        {
            var eventQuery =new EventQuery
            {
                EventId = initialLoadFinished.EventId,
                CreatedAt = initialLoadFinished.CreateAt,
                ErrorMessage = state.FailedEvents
                    .FirstOrDefault(f => f.EventId == initialLoadFinished.EventId)
                    ?.ErrorMessage,
                Status = ""
            };
            
            var transformationState = import.TransformationState?
                .FirstOrDefault(p => p.CausationId == initialLoadFinished.EventId);
            if (transformationState is null) return Enumerable.Empty<EventQuery>();
            
            var transformationEvents = GetTransformationEvents(import, initialLoadFinished);

            eventQuery.State = new StateQuery
            {
                Name = StepsName.Transformation,
                CorrelationId = transformationState.CorrelationId,
                CausationId = transformationState.CausationId,
                CreateAt = transformationState.CreateAt,
                Status = "",
                TotalCount = transformationState.TotalCount,
                Events = transformationEvents
            };
            
            events.Add(eventQuery);
        }

        return events;
    }

    private IEnumerable<EventQuery> GetTransformationEvents(ImportFlowProcess import,
        InitialLoadFinished @event)
    {
        var events = new List<EventQuery>();
        
        var state = import.TransformationState?
            .FirstOrDefault(p => p.CausationId == @event.EventId);
        if (state is null)
        {
            return events;
        };
        
        foreach (var transformationFinished in state.Events)
        {
            var eventQuery =new EventQuery
            {
                EventId = transformationFinished.EventId,
                CreatedAt = transformationFinished.CreateAt,
                ErrorMessage = state.FailedEvents
                    .FirstOrDefault(f => f.EventId == transformationFinished.EventId)
                    ?.ErrorMessage,
                Status = ""
            };
            
            var dataExportState = import.DataExportState?
                .FirstOrDefault(p => p.CausationId == transformationFinished.EventId);
            if (dataExportState is null) return Enumerable.Empty<EventQuery>();
            
            var dataExportEvents = GetDataExportEvents(import, transformationFinished);

            eventQuery.State = new StateQuery
            {
                Name = StepsName.DateExport,
                CorrelationId = dataExportState.CorrelationId,
                CausationId = dataExportState.CausationId,
                CreateAt = dataExportState.CreateAt,
                Status = "",
                TotalCount = dataExportState.TotalCount,
                Events = dataExportEvents
            };
            
            events.Add(eventQuery);
        }

        return events;
    }

    private IEnumerable<EventQuery> GetDataExportEvents(ImportFlowProcess import,
        TransformationFinished @event)
    {
        var events = new List<EventQuery>();
        
        var state = import.DataExportState?
            .FirstOrDefault(p => p.CausationId == @event.EventId);
        if (state is null)
        {
            return events;
        };
        
        foreach (var dataExported in state.Events)
        {
            var eventQuery =new EventQuery
            {
                EventId = dataExported.EventId,
                CreatedAt = dataExported.CreateAt,
                ErrorMessage = state.FailedEvents
                    .FirstOrDefault(f => f.EventId == dataExported.EventId)
                    ?.ErrorMessage,
                Status = ""
            };
            events.Add(eventQuery);
        }

        return events;
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
            };

            result.Add(query);
        }

        return result;
    }

    private string GetStatus(ImportFlowProcess import)
    {
        var timeDifference = DateTime.Now - import.CreateAt;
        if (timeDifference.Minutes > 5 && import.DataExportState?.Count() == 0)
        {
            return ImportState.Failed.ToString();
        }

        if (import.DataExportState?.Count() == 0)
        {
            return ImportState.Processing.ToString();
        }


        var isDownloadSucceed = import.DownloadedFilesState.Status == ImportState.Completed.ToString();


        var filesTotalCount = import.DownloadedFilesState.TotalCount;
        var isInitialLoadSucceed = import.InitialLoadState != null &&
                                   import.InitialLoadState.Count() == filesTotalCount &&
                                   import.InitialLoadState.All(p => p.Status == ImportState.Completed.ToString());

        var transformationCount = import.InitialLoadState?.Sum(p => p.TotalCount);
        var isTransformationSucceed = import.TransformationState != null &&
                                      import.TransformationState.Count() == transformationCount &&
                                      import.TransformationState.All(p => p.Status == ImportState.Completed.ToString());

        var dateExportCount = import.TransformationState?.Sum(p => p.TotalCount);
        var isDateExportSucceed = import.DataExportState != null &&
                                  import.DataExportState.Count() == dateExportCount &&
                                  import.DataExportState.All(p => p.Status == ImportState.Completed.ToString());


        var isSucceed = isDownloadSucceed && isInitialLoadSucceed && isTransformationSucceed && isDateExportSucceed;

        if (isSucceed)
        {
            return ImportState.Completed.ToString();
        }

        var anyDataExportCompleted = import.DataExportState?
            .Any(p => p.Status == ImportState.Completed.ToString()) ?? false;

        if (!anyDataExportCompleted && timeDifference.Minutes > 5)
        {
            return ImportState.Failed.ToString();
        }

        if (anyDataExportCompleted && timeDifference.Minutes > 5)
        {
            return ImportState.PartiallyFailed.ToString();
        }

        return ImportState.Processing.ToString();
    }

    private string GetStatus(Domain.State<SupplierFilesDownloaded> state, Guid eventId)
    {
        if (state.SucceedEvents?.FirstOrDefault(p => p == eventId) != null)
        {
            return ImportState.Completed.ToString();
        }

        if (state.FailedEvents.FirstOrDefault(p => p.EventId == eventId) != null)
        {
            return ImportState.Failed.ToString();
        }

        return ImportState.Processing.ToString();
    }

    private async Task StartAsync(ImportFlowProcessInfo info)
    {
        var importFlow = ImportFlowProcess.Start(info);
        await repository.AddAsync(importFlow);
    }
}