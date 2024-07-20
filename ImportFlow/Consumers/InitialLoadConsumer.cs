using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class InitialLoadConsumer(IStateRepository<InitialLoadFinished> repository)
    : IMessageConsumer<SupplierFilesDownloaded>
{
    public async Task Consume(ConsumeContext<SupplierFilesDownloaded> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

        var state = Domain.State<InitialLoadFinished>.Create(
            StepsName.InitialLoad,
            context.Message.CorrelationId,
            context.Message.CausationId, 5);

        await repository.AddAsync(state);

        if (number == 1)
        {
            throw new Exception("Something went wrong");
        }

        for (var i = 0; i < 5; i++)
        {
            var @event = new InitialLoadFinished
            {
                CausationId = context.Message.EventId,
                CorrelationId = context.Message.CorrelationId,
                Number = i + 1
            };
            // ...

            await repository.PublishedAsync(@event);
            await context.Publish(@event);
        }
    }
}