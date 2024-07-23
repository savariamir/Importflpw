using ImportFlow.Framework.Domain;
using ImportFlow.Framework.QueryModels;

namespace ImportFlow.Framework;

public class QueryModelBuilder
{
    public ImportFlowQueryModel Build(Domain.ImportFlow import)
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
                Events = CreateEventsV2(import, import.DownloadedFilesState)
            }
        };
        
        var statusVisitor = new StatusVisitor();
        statusVisitor.Visit(model);
        
        var messagesVisitor = new LogVisitor();
        messagesVisitor.Visit(model);

        return model;
    }

    public IEnumerable<ImportFlowQueryModel> GetImportFlowList(IEnumerable<Domain.ImportFlow> imports)
    {
        var result = new List<ImportFlowQueryModel>();
        var visitor = new LogVisitor();
        foreach (var import in imports)
        {
            var query1 = Build(import);
            visitor.Visit(query1);
            // var query = new ImportFlowQueryModel
            // {
            //     ImportFlowProcessId = import.ImportFlowProcessId,
            //     PlatformId = import.PlatformId,
            //     SupplierId = import.SupplierId,
            //     CreateAt = import.CreateAt,
            //     UpdatedAt = import.UpdatedAt,
            //     Status = GetStatus(import),
            //     // Messages = new []{ "Message 1", "Message 2"}
            // };
            result.Add(query1);
        }

        return result;
    }

    private IEnumerable<EventQueryModel> CreateEventsV2(Domain.ImportFlow import, State currentState)
    {
        var events = new List<EventQueryModel>();
        foreach (var @event in currentState.Events)
        {
            var eventQuery = new EventQueryModel
            {
                EventId = @event.EventId,
                CreatedAt = @event.CreatedAt,
                EventName = @event.GetType().Name,
                Retry = @event.Retry != null? new RetryQueryModel
                {
                    Reason = @event.Retry.Reason,
                    RetryBy = @event.Retry.RetryBy,
                    CausationId = @event.Retry.CausationId
                }: null,
                FailedEvents = currentState.FailedEvents.Where(p => p.EventId == @event.EventId).Select(p =>
                    new MessageQueryModel
                    {
                        EventId = p.EventId,
                        CreatedAt = p.CreatedAt,
                        ErrorMessage = p.ErrorMessage
                    }),
                State = CreateStateQueryModel(currentState.Name, currentState.FailedEvents, @event.EventId)
            };

            var nextState = GetNextState(import, currentState.Name)
                ?.FirstOrDefault(p => p.CausationId == @event.EventId);
            if (nextState is not null)
            {
                var initialLoadEvents = CreateEventsV2(import, nextState);

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

    private static IEnumerable<State>? GetNextState(Domain.ImportFlow import, string currentStateName)
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

    private static StateQueryModel CreateStateQueryModel(string stepName, IEnumerable<Message> failedEvents,
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