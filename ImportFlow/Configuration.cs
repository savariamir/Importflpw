using ImportFlow.Events;
using ImportFlow.Framework;
using ImportFlow.Infrastructure.Consumers;
using MassTransit;

namespace ImportFlow;

public static class Configuration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessageConsumer<SupplierFilesDownloaded>, InitialLoadConsumer>();
        services.AddScoped<IMessageConsumer<InitialLoadFinished>, TransformationConsumer>();
        services.AddScoped<IMessageConsumer<TransformationFinished>, DataExportConsumer>();
        
        services.AddMassTransit(mt =>
        {
            mt.AddConsumer<BaseConsumer<SupplierFilesDownloaded>>();
            mt.AddConsumer<BaseConsumer<InitialLoadFinished>>();
            mt.AddConsumer<BaseConsumer<TransformationFinished>>();

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
                    ec.ConfigureConsumer<BaseConsumer<SupplierFilesDownloaded>>(context);
                });
                
                cfg.ReceiveEndpoint("transformation", ec =>
                {
                    ec.DefaultMessageTimeToLive = TimeSpan.FromDays(5);

                    ec.UseMessageRetry(x => x.Interval(5, TimeSpan.FromSeconds(1)));
                    ec.UseDelayedRedelivery(x =>
                    {
                        x.Incremental(5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                    });
                    ec.ConfigureConsumer<BaseConsumer<InitialLoadFinished>>(context);
                });
                
                cfg.ReceiveEndpoint("data-export", ec =>
                {
                    ec.DefaultMessageTimeToLive = TimeSpan.FromDays(5);

                    ec.UseMessageRetry(x => x.Interval(5, TimeSpan.FromSeconds(1)));
                    ec.UseDelayedRedelivery(x =>
                    {
                        x.Incremental(5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                    });
                    ec.ConfigureConsumer<BaseConsumer<TransformationFinished>>(context);
                });
            });
        });

        return services;
    }
}