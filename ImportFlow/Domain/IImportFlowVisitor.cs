using ImportFlow.Events;

namespace ImportFlow.Domain;

public interface IImportFlowVisitor
{
    void Visit(ImportFlowProcess importFlow);
    void Visit<TEvent>(State<TEvent> state) where TEvent : ImportEvent;

    void Visit<TEvent>(EventNode<TEvent> @event) where TEvent : ImportEvent;
}

public class StatusVisitorVisitor : IImportFlowVisitor
{
    public void Visit(ImportFlowProcess importFlow)
    {
        importFlow.Tree.Accept(this);
    }

    public void Visit<TEvent>(EventNode<TEvent> @event) where TEvent : ImportEvent
    {
        if (@event?.State is null)
        {
            return;
        }

        foreach (var stateEvent in @event.State.EventsInfo)
        {
            stateEvent.Accept(this);
        }


        var statuses = @event.State.EventsInfo.Select(p => p.Status).ToList();

        if (statuses.Contains(ImportState.Processing))
        {
            @event.Status = ImportState.Processing;
        }

        if (statuses.All(s => s == ImportState.Completed))
        {
            @event.Status = ImportState.Completed;
        }

        if (statuses.All(s => s == ImportState.Failed))
        {
            @event.Status = ImportState.Failed;
        }

        @event.Status = ImportState.PartiallyFailed;
    }

    public void Visit<TEvent>(State<TEvent> state) where TEvent : ImportEvent
    {
        foreach (var stateEvent in state.EventsInfo)
        {
            stateEvent.Accept(this);
        }

        var statuses = state.EventsInfo.Select(p => p.Status).ToList();

        if (statuses.Contains(ImportState.Processing))
        {
            state.SetStatus(ImportState.Processing);
        }

        if (statuses.All(s => s == ImportState.Completed))
        {
            state.SetStatus(ImportState.Completed);
        }

        if (statuses.All(s => s == ImportState.Failed))
        {
            state.SetStatus(ImportState.Failed);
        }

        state.SetStatus(ImportState.PartiallyFailed);
    }
}

public class BuildTreeVisitor : IImportFlowVisitor
{
    private ImportFlowProcess _import;
    private Guid _currentEventId;
    private string _currentStep;

    public void Visit(ImportFlowProcess importFlow)
    {
        _currentStep = StepsName.SupplierFiles;
        _import = importFlow;
        importFlow.DownloadedFilesState.Accept(this);

        var events = new List<EventNode<SupplierFilesDownloaded>>();

        var state = State<SupplierFilesDownloaded>.Create(
            _currentStep,
            importFlow.DownloadedFilesState.CorrelationId,
            importFlow.DownloadedFilesState.CorrelationId,
            importFlow.DownloadedFilesState.TotalCount);
        foreach (var node in importFlow.DownloadedFilesState.EventsInfo)
        {
            _currentEventId = node.Event.EventId;
            node.State = State<SupplierFilesDownloaded>.Create(
                ImportProcess.GetNextName(_currentStep),
                importFlow.DownloadedFilesState.CorrelationId,
                importFlow.DownloadedFilesState.CorrelationId,
                importFlow.DownloadedFilesState.TotalCount);

            node.Accept(this);
        }

        importFlow.Tree = state;
    }

    public void Visit<TEvent>(EventNode<TEvent> @event) where TEvent : ImportEvent
    {
        if (@event?.State is null)
        {
            return;
        }
        
        foreach (var stateEvent in @event.State.EventsInfo)
        {
            stateEvent.Accept(this);
        }
    }

    public void Visit<TEvent>(State<TEvent> state) where TEvent : ImportEvent
    {
        foreach (var stateEvent in state.EventsInfo)
        {
            stateEvent.Accept(this);
        }
    }
}