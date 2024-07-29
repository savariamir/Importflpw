using ImportFlow.Domain;
using ImportFlow.Infrastructure.QueryModels;

namespace ImportFlow.Infrastructure;

public abstract class StatusBuilder
{
    public static ImportStatus Build(State domainState, List<StateQueryModel> nextStates)
    {
        if (ImportStepService.IsStepLeaf(domainState.Name))
        {
            return domainState.Status;
        }

        var isStateExpired = (DateTime.Now - domainState.CreatedAt).Minutes > 1;
        if (nextStates is null)
        {
            return isStateExpired ? ImportStatus.Failed : ImportStatus.Processing;
        }
        
        var statuses = nextStates.Select(p => p.Status).ToList();
        if (statuses.Any(s => s == ImportStatus.Processing.ToString()))
        {
            return ImportStatus.Processing;
        }

        var isAllEventsAdded = nextStates.Count ==domainState.TotalCount;
        if (statuses.All(s => s == ImportStatus.Completed.ToString()) && isAllEventsAdded)
        {
            return ImportStatus.Completed;
        }

        if (statuses.All(s => s == ImportStatus.Failed.ToString()) && isAllEventsAdded && isStateExpired)
        {
           return ImportStatus.Failed;
        }

        if (isAllEventsAdded && isStateExpired)
        {
            return ImportStatus.PartialSuccess;
        }

        return ImportStatus.Processing;
    }
}