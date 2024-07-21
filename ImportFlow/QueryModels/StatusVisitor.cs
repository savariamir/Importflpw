using ImportFlow.Domain;

namespace ImportFlow.QueryModels;

public class StatusVisitor : IVisitor
{
    public void Visit(ImportFlowQuery importFlow)
    {
        importFlow.State?.Accept(this);
        importFlow.Status = importFlow.State?.Status;
    }

    public void Visit(StateQuery state)
    {
        foreach (var eventQuery in state.Events)
        {
            eventQuery.Accept(this);
        }
    }


    public void Visit(EventQuery eventQuery)
    {
        eventQuery.State?.Accept(this);

        if (eventQuery.State?.Events == null)
        {
            return;
        }

        var statuses = eventQuery.State.Events.Select(p => p.Status).ToList();

        if (statuses.Contains(ImportState.Processing))
        {
            eventQuery.Status = ImportState.Processing;
        }

        if (statuses.All(s => s == ImportState.Completed))
        {
            eventQuery.Status = ImportState.Completed;
        }

        if (statuses.All(s => s == ImportState.Failed))
        {
            eventQuery.Status = ImportState.Failed;
        }

        eventQuery.Status = ImportState.PartiallyFailed;
    }
}