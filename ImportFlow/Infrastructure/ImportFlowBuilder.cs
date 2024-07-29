using ImportFlow.Domain;
using ImportFlow.Events;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Infrastructure;

public abstract class QueryModelBuilder
{
    public static ImportFlowQueryModel Build(ImportProcess import)
    {
        var initialState = import.States.First(p => p.IsInitialState);
        var model = new ImportFlowQueryModel
        {
            ImportFlowProcessId = import.Id,
            PlatformId = import.PlatformId,
            SupplierId = import.SupplierId,
            CreateAt = import.CreateAt,
            UpdatedAt = import.UpdatedAt,
            State = new StateQueryModel
            {
                Name = initialState.Name,
                CorrelationId = initialState.CorrelationId,
                CausationId = initialState.CausationId,
                CreateAt = initialState.CreatedAt,
                TotalCount = initialState.TotalCount,
                Events = CreateEvents(initialState),
                NextStates = CreateNextStates(import, initialState)
            },
        };

        var statusVisitor = new StatusVisitor();
        statusVisitor.Visit(model);

        var messagesVisitor = new MessageVisitor();
        messagesVisitor.Visit(model);

        return model;
    }

    private static IEnumerable<StateQueryModel> CreateNextStates(ImportProcess import, State currentState)
    {
        var states = GetNextStates(import, currentState);
        if (states.Count == 0)
        {
            return [];
        }

        var nextStates = new List<StateQueryModel>();

        if (!currentState.HasEvents)
        {
            nextStates.AddRange(states.Select(state => new StateQueryModel
            {
                Name = state.Name,
                Status = state.Status.ToString(),
                CorrelationId = state.CorrelationId,
                CausationId = state.CausationId,
                CreateAt = state.CreatedAt,
                TotalCount = state.TotalCount,
                Events = CreateEvents(state),
                NextStates = CreateNextStates(import, state)
            }));
        }
        else
        {
            foreach (var t in currentState.Events)
            {
                var state = states.FirstOrDefault(p => p.CausationId == t.EventId);
                if (state is null) continue;

                nextStates.Add(new StateQueryModel
                {
                    Name = state.Name,
                    Status = state.Status.ToString(),
                    CorrelationId = state.CorrelationId,
                    CausationId = state.CausationId,
                    CreateAt = state.CreatedAt,
                    TotalCount = state.TotalCount,
                    Events = CreateEvents(state),
                    NextStates = CreateNextStates(import, state)
                });
            }
        }


        return nextStates;
    }

    public static IEnumerable<ImportFlowQueryModel> GetImportFlowList(IEnumerable<ImportProcess> imports)
    {
        var result = new List<ImportFlowQueryModel>();
        var visitor = new MessageVisitor();
        foreach (var import in imports)
        {
            var queryModel = Build(import);
            visitor.Visit(queryModel);
            result.Add(queryModel);
        }

        return result;
    }

    private static IEnumerable<EventQueryModel> CreateEvents(State state)
    {
        return state.Events.Select(@event => new EventQueryModel
            {
                EventId = @event.EventId,
                CreatedAt = @event.CreatedAt,
                EventName = @event.GetType().Name,
                Retry = CreateRetry(@event.Retry),
                FailedEvents = GetFailedEvents(state.FailedEvents, @event.EventId),
            })
            .ToList();
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

    private static List<State> GetNextStates(ImportProcess import, State currentState)
    {
        if (currentState.Events is null) return [];
        var events = currentState.Events.Select(p => p.EventId);

        var nextState = import.GetNextState(currentState.Name);

        return import.States.Where(p => p.Name == nextState && events.Contains(p.CausationId)).ToList();
    }
}