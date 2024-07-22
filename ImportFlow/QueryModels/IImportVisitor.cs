namespace ImportFlow.QueryModels;

public interface IVisitor
{
    void Visit(StateQuery state);

    void Visit(ImportFlowQuery state);

    void Visit(EventQuery @event);
}