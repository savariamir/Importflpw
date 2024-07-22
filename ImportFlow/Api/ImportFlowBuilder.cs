using ImportFlow.Domain;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.QueryModels;

namespace ImportFlow.Api;

public class ImportFlowBuilder
{
    public ImportFlowQueryModel Build(ImportFlowV2 import)
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
                CreateAt = import.DownloadedFilesState.CreateAt,
                TotalCount = import.DownloadedFilesState.TotalCount,
                Events = CreateEventsV2(import, import.DownloadedFilesState)
            }
        };
        
        var visitor = new StatusVisitor();
        visitor.Visit(model);

        return model;
    }

    public IEnumerable<ImportFlowQueryModel> GetImportFlowList(IEnumerable<ImportFlowV2> imports)
    {
        var result = new List<ImportFlowQueryModel>();
        var visitor = new MessageVisitor();
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

            query1.Messages = visitor.GetMessages;
            result.Add(query1);
        }

        return result;
    }

    private string GetStatus(ImportFlowV2 import)
    {
        var timeDifference = DateTime.Now - import.CreateAt;
        if (timeDifference.Minutes > 1 && import.DataExportState?.Count() == 0)
        {
            return ImportStatus.Failed.ToString();
        }

        if (import.DataExportState?.Count() == 0)
        {
            return ImportStatus.Processing.ToString();
        }


        var isDownloadSucceed = import.DownloadedFilesState.Status == ImportStatus.Completed;


        var filesTotalCount = import.DownloadedFilesState.TotalCount;
        var isInitialLoadSucceed = import.InitialLoadState != null &&
                                   import.InitialLoadState.Count() == filesTotalCount &&
                                   import.InitialLoadState.All(p => p.Status == ImportStatus.Completed);

        var transformationCount = import.InitialLoadState?.Sum(p => p.TotalCount);
        var isTransformationSucceed = import.TransformationState != null &&
                                      import.TransformationState.Count() == transformationCount &&
                                      import.TransformationState.All(p => p.Status == ImportStatus.Completed);

        var dateExportCount = import.TransformationState?.Sum(p => p.TotalCount);
        var isDateExportSucceed = import.DataExportState != null &&
                                  import.DataExportState.Count() == dateExportCount &&
                                  import.DataExportState.All(p => p.Status == ImportStatus.Completed);


        var isSucceed = isDownloadSucceed && isInitialLoadSucceed && isTransformationSucceed && isDateExportSucceed;

        if (isSucceed)
        {
            return ImportStatus.Completed.ToString();
        }

        var anyDataExportCompleted = import.DataExportState?
            .Any(p => p.Status == ImportStatus.Completed) ?? false;

        if (!anyDataExportCompleted && timeDifference.Minutes > 1)
        {
            return ImportStatus.Failed.ToString();
        }

        if (anyDataExportCompleted && timeDifference.Minutes > 1)
        {
            return ImportStatus.PartiallyFailed.ToString();
        }

        return ImportStatus.Processing.ToString();
    }


    private IEnumerable<EventQueryModel> CreateEventsV2(ImportFlowV2 import, StateV2 currentState)
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
                    CreateAt = nextState.CreateAt,
                    Status = nextState.Status.ToString(),
                    TotalCount = nextState.TotalCount,
                    Events = initialLoadEvents
                };
            }

            events.Add(eventQuery);
        }

        return events;
    }

    // private long GetDuration()
    // {
    //     return 
    // }

    private static IEnumerable<StateV2>? GetNextState(ImportFlowV2 import, string currentStateName)
    {
        return currentStateName switch
        {
            StepsName.SupplierFiles => import.InitialLoadState,
            StepsName.InitialLoad => import.TransformationState,
            StepsName.Transformation => import.DataExportState,
            StepsName.DateExport => Enumerable.Empty<StateV2>(),
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
                : ImportStatus.Processing.ToString()
        };
    }
}