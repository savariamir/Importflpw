using ImportFlow.Domain;
using ImportFlow.Events;

namespace ImportFlow;

public static class ImportProcess
{
    public static string GetTypeName<TEvent>(TEvent @event) where TEvent : ImportEvent
    {
        return @event switch
        {
            SupplierFilesDownloaded => StepsName.SupplierFiles,
            InitialLoadFinished => StepsName.InitialLoad,
            TransformationFinished => StepsName.Transformation,
            DataExported => StepsName.DateExport,
            _ => throw new InvalidOperationException()
        };
    }
    
    
    public static string GetNextName(string stateName)
    {
        return stateName switch
        {
            StepsName.SupplierFiles =>  StepsName.InitialLoad,
            StepsName.InitialLoad =>  StepsName.Transformation,
            StepsName.Transformation =>  StepsName.DateExport,
            StepsName.DateExport =>  string.Empty,
            _ => throw new InvalidOperationException()
        };
    }
}