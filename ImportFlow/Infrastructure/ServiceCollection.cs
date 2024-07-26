using ImportFlow.Application;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using ImportFlow.Infrastructure.InMemoryRepositories;

namespace ImportFlow.Infrastructure;

public static class ServiceCollection
{
    public static IServiceCollection AddImportFlowServices(this IServiceCollection services)
    {
        services.AddTransient<MessageRePublisher>();
        services.AddTransient<MessagePublisher>();
        services.AddTransient<ImportMonitoring>();


        services.AddSingleton<IImportFlowRepository, InMemoryImportFlowRepository>();
        services.AddSingleton<IStateRepository<ImportEvent>, InMemoryStateRepository<ImportEvent>>();


        return services;
    }
}