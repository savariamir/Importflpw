using ImportFlow.Domain;
using ImportFlow.Events;

namespace ImportFlow.QueryModels;

public interface IVisitor
{
    void Visit(StateQuery state);

    void Visit(ImportFlowQuery state);

    void Visit(EventQuery @event);
}

public interface IImportFlowVisitor
{
    void Visit(ImportFlowProcess importFlow);
    void Visit<TEvent>(State<TEvent> statr) where TEvent: ImportEvent;
}