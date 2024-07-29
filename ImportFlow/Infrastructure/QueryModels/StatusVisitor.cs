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
        if (ImportStepService.IsStepLeaf(state.Name))
        {
            return;
        }

        var isStateExpired = (DateTime.Now - state.CreateAt).Minutes > 1;
        if (state.NextStates is null)
        {
            state.Status = isStateExpired ? ImportStatus.Failed.ToString() : ImportStatus.Processing.ToString();
            return;
        }

        foreach (var nextState in state.NextStates)
        {
            nextState.Accept(this);
        }

        var statuses = state.NextStates.Select(p => p.Status).ToList();
        if (statuses.Any(s => s == ImportStatus.Processing.ToString()))
        {
            state.Status = ImportStatus.Processing.ToString();
            return;
        }

        var isAllEventsAdded = state.NextStates?.Count() == state.TotalCount;
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
    }
}