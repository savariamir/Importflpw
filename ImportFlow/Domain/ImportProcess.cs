namespace ImportFlow.Domain;

public class ImportProcess
{
    public Guid Id { get; private set; }

    public int PlatformId { get; private set; }

    public int? SupplierId { get; private set; }

    public DateTime CreateAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IEnumerable<State> States { get; private set; }

    private Dictionary<string, string> Transitions { get; set; }

    public string GetNextState(string step)
    {
        return Transitions.TryGetValue(step, out var state) ? state : string.Empty;
    }

    public string GetInitialStateName()
    {
        if (Transitions.Count > 0)
        {
            return Transitions.Keys.First();
        }

        throw new AggregateException();
    }

    private ImportProcess(ImportProcessOptions options)
    {
        Id = options.CorrelationId;
        PlatformId = options.PlatformId;
        SupplierId = options.SupplierId;
        CreateAt = DateTime.Now;
        Transitions = options.Transitions;
    }

    public static ImportProcess NewImport(ImportProcessOptions options)
    {
        return new ImportProcess(options);
    }

    public void Set(List<State> states)
    {
        States = states;
    }
}