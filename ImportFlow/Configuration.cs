using ImportFlow.Consumers;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow;

public static class Configuration
{
    public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(mt =>
        {
            mt.AddConsumer<AggregatorConsumer<SupplierFilesDownloaded>>();
            mt.AddConsumer<AggregatorConsumer<InitialLoadFinished>>();
            mt.AddConsumer<AggregatorConsumer<TransformationFinished>>();

            mt.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("AzureServiceBus"));
                cfg.ReceiveEndpoint("initial-load", ec =>
                {
                    ec.DefaultMessageTimeToLive = TimeSpan.FromDays(5);

                    ec.UseMessageRetry(x => x.Interval(5, TimeSpan.FromSeconds(1)));
                    ec.UseDelayedRedelivery(x =>
                    {
                        x.Incremental(5, TimeSpan.FromMicroseconds(10), TimeSpan.FromMicroseconds(5));
                    });
                    ec.ConfigureConsumer<AggregatorConsumer<SupplierFilesDownloaded>>(context);
                });
                
                cfg.ReceiveEndpoint("transformation", ec =>
                {
                    ec.DefaultMessageTimeToLive = TimeSpan.FromDays(5);

                    ec.UseMessageRetry(x => x.Interval(5, TimeSpan.FromSeconds(1)));
                    ec.UseDelayedRedelivery(x =>
                    {
                        x.Incremental(5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                    });
                    ec.ConfigureConsumer<AggregatorConsumer<InitialLoadFinished>>(context);
                });
                
                cfg.ReceiveEndpoint("data-export", ec =>
                {
                    ec.DefaultMessageTimeToLive = TimeSpan.FromDays(5);

                    ec.UseMessageRetry(x => x.Interval(5, TimeSpan.FromSeconds(1)));
                    ec.UseDelayedRedelivery(x =>
                    {
                        x.Incremental(5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                    });
                    ec.ConfigureConsumer<AggregatorConsumer<TransformationFinished>>(context);
                });
            });
        });

        return services;
    }
}