using ImportFlow.Domain;
using ImportFlow.Events;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Infrastructure.Query;

public abstract class QueryModelBuilder
{
    public static ImportFlowQueryModel Build(ImportFlowProcess import)
    {
        var model = new ImportFlowQueryModel
        {
            ImportFlowProcessId = import.Id,
            PlatformId = import.PlatformId,
            SupplierId = import.SupplierId,
            CreateAt = import.CreateAt,
            UpdatedAt = import.UpdatedAt,
            State = new StateQueryModel
            {
                Name = StepsName.SupplierFiles,
                CorrelationId = import.DownloadedFilesState.CorrelationId,
                CausationId = import.DownloadedFilesState.CausationId,
                CreateAt = import.DownloadedFilesState.CreatedAt,
                TotalCount = import.DownloadedFilesState.TotalCount,
                Events = CreateEvents(import, import.DownloadedFilesState)
            }
        };

        var statusVisitor = new StatusVisitor();
        statusVisitor.Visit(model);

        var messagesVisitor = new LogVisitor();
        messagesVisitor.Visit(model);
        
        var lastUpdateVisitor = new LastUpdateVisitor();
        lastUpdateVisitor.Visit(model);

        return model;
    }
    
    public static IEnumerable<ImportFlowQueryModel> GetImportFlowList(IEnumerable<ImportFlowProcess> imports)
    {
        var result = new List<ImportFlowQueryModel>();
        var visitor = new LogVisitor();
        foreach (var import in imports)
        {
            var queryModel = Build(import);
            visitor.Visit(queryModel);
            result.Add(queryModel);
        }

        return result;
    }

    private static IEnumerable<EventQueryModel> CreateEvents(ImportFlowProcess import, State currentState)
    {
        var events = new List<EventQueryModel>();
        foreach (var @event in currentState.Events)
        {
            var eventQuery = new EventQueryModel
            {
                EventId = @event.EventId,
                CreatedAt = @event.CreatedAt,
                EventName = @event.GetType().Name,
                Retry = CreateRetry(@event.Retry),
                FailedEvents = GetFailedEvents(currentState.FailedEvents, @event.EventId),
                State = CreateStateQueryModel(currentState.Name, currentState.FailedEvents, @event.EventId)
            };

            var nextState = GetNextState(import, currentState.Name)
                ?.FirstOrDefault(p => p.CausationId == @event.EventId);
            if (nextState is not null)
            {
                var initialLoadEvents = CreateEvents(import, nextState);

                eventQuery.State = new StateQueryModel
                {
                    Name = nextState.Name,
                    CorrelationId = nextState.CorrelationId,
                    CausationId = nextState.CausationId,
                    CreateAt = nextState.CreatedAt,
                    Status = nextState.Status.ToString(),
                    TotalCount = nextState.TotalCount,
                    Events = initialLoadEvents
                };
            }

            events.Add(eventQuery);
        }

        return events;
    }

    private static RetryQueryModel? CreateRetry(Retry? retry)
    {
        return retry != null
            ? new RetryQueryModel
            {
                Reason = retry.Reason,
                RetryBy = retry.RetryBy,
                CausationId = retry.CausationId
            }
            : null;
    }

    private static IEnumerable<FailedEventQueryModel>? GetFailedEvents(IEnumerable<FailedEvent>? failedEvents,
        Guid eventId)
    {
        return failedEvents?.Where(p => p.EventId == eventId).Select(p =>
            new FailedEventQueryModel
            {
                EventId = p.EventId,
                CreatedAt = p.CreatedAt,
                ErrorMessage = p.ErrorMessage
            });
    }

    private static IEnumerable<State>? GetNextState(ImportFlowProcess import, string currentStateName)
    {
        return currentStateName switch
        {
            StepsName.SupplierFiles => import.InitialLoadState,
            StepsName.InitialLoad => import.TransformationState,
            StepsName.Transformation => import.DataExportState,
            StepsName.DateExport => Enumerable.Empty<State>(),
            _ => throw new ArgumentOutOfRangeException(nameof(currentStateName), currentStateName, null)
        };
    }

    private static StateQueryModel CreateStateQueryModel(string stepName, IEnumerable<FailedEvent> failedEvents,
        Guid eventId)
    {
        return new StateQueryModel
        {
            Name = ImportProcess.GetNextName(stepName),
            Status = failedEvents
                .FirstOrDefault(f => f.EventId == eventId) != null
                ? ImportStatus.Failed.ToString()
                : ImportStatus.Processing.ToString(),
            CreateAt = DateTime.Now
        };
    }
}