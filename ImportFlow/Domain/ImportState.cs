namespace ImportFlow.Domain;

public enum ImportState
{
    Processing = 1,
    Completed = 2,
    PartiallyFailed = 3,
    Failed = 4
}