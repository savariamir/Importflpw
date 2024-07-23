using ImportFlow.Domain;

namespace ImportFlow.Infrastructure.QueryModels;

public class StatusVisitor : IVisitor
{
    public void Visit(ImportFlowQueryModel importFlowQuery)
    {
        importFlowQuery.State?.Accept(this);
        importFlowQuery.Status = importFlowQuery.State?.Status;
    }

    public void Visit(StateQueryModel state)
    {
        var isStateExpired = (DateTime.Now - state.CreateAt).Minutes > 1;
        if (state.Events is null)
        {
            state.Status = isStateExpired ? ImportStatus.Failed.ToString() : ImportStatus.Processing.ToString();
            return;
        }

        var isAllEventsAdded = state.Events.Count() == state.TotalCount;
        if (IsStepLeaf(state) && isAllEventsAdded)
        {
            state.Status = ImportStatus.Completed.ToString();
            return;
        }

        foreach (var eventQuery in state.Events)
        {
            eventQuery.Accept(this);
        }

        var statuses = state.Events.Select(p => p.State?.Status).ToList();

        if (statuses.Any(s => s == ImportStatus.Processing.ToString()))
        {
            state.Status = ImportStatus.Processing.ToString();
            return;
        }

        if (statuses.All(s => s == ImportStatus.Completed.ToString()) && isAllEventsAdded)
        {
            state.Status = ImportStatus.Completed.ToString();
            return;
        }

        if (statuses.All(s => s == ImportStatus.Failed.ToString()) && isAllEventsAdded && isStateExpired)
        {
            state.Status = ImportStatus.Failed.ToString();
            return;
        }

        if (isAllEventsAdded && isStateExpired)
        {
            state.Status = ImportStatus.PartialSuccess.ToString();
            return;
        }

        state.Status = ImportStatus.Processing.ToString();
    }

    public void Visit(EventQueryModel eventQueryModel)
    {
        eventQueryModel.State?.Accept(this);
    }

    private static bool IsStepLeaf(StateQueryModel state)
    {
        return state.Name == StepsName.DateExport;
    }
}

public class LastUpdateVisitor : IVisitor
{
    private DateTime? _lastUpdate;

    public void Visit(StateQueryModel state)
    {
        if (state.Events is null)
        {
            return;
        }

        foreach (var eventQuery in state.Events)
        {
            eventQuery.Accept(this);
        }
    }

    public void Visit(ImportFlowQueryModel importFlow)
    {
        importFlow.State?.Accept(this);
    }

    public void Visit(EventQueryModel @event)
    {
        if (_lastUpdate is null)
        {
            _lastUpdate = @event.CreatedAt;
        }
        else if(_lastUpdate <  @event.CreatedAt)
        {
            _lastUpdate = @event.CreatedAt;
        }
    }
}