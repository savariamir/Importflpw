namespace ImportFlow.Domain;

public class ImportFlowManager
{
    private readonly Dictionary<string, string> _transitions = new();
    
    public void AddTransition(string fromStep, string toStep)
    {
        _transitions[fromStep] = toStep;
    }

    public Dictionary<string, string> GetTransitions()
    {
        return _transitions;
    }
}