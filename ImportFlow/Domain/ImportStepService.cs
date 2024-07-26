using ImportFlow.Events;

namespace ImportFlow.Domain;

public static class ImportStepService
{
    public static bool IsStepLeaf(string stepName)
    {
        return stepName == StepsName.DateExport;
    }
    public static string GetNextName(string stateName)
    {
        return stateName switch
        {
            StepsName.PushApi =>  StepsName.InitialLoad,
            StepsName.InitialLoad =>  StepsName.Transformation,
            StepsName.Transformation =>  StepsName.DateExport,
            StepsName.DateExport =>  string.Empty,
            _ => throw new InvalidOperationException()
        };
    }
    
    public static string GetStepName<TEvent>(TEvent @event) where TEvent : ImportEvent
    {
        return @event switch
        {
            SupplierFilesDownloaded => StepsName.InitialLoad,
            InitialLoadFinished => StepsName.Transformation,
            TransformationFinished => StepsName.DateExport,
            _ => throw new InvalidOperationException()
        };
    }
}