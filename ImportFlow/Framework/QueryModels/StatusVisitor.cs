using ImportFlow.Framework.Domain;

namespace ImportFlow.Framework.QueryModels;

public class StatusVisitor : IVisitor
{
    public void Visit(ImportFlowQueryModel importFlow)
    {
        importFlow.State?.Accept(this);
        importFlow.Status = importFlow.State?.Status;
    }
    
    public void Visit(StateQueryModel state)
    {
        var isStateExpired = (DateTime.Now - state.CreateAt).Minutes > 1;
        if (state.Events is null)
        {
            state.Status = isStateExpired ?  ImportStatus.Failed.ToString(): ImportStatus.Processing.ToString();
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
    
    private bool IsStepLeaf(StateQueryModel state)
    {
        return state.Name == StepsName.DateExport;
    }
}