using System;
using System.Threading;
using System.Threading.Tasks;
using EventContracts;
using MassTransit;

namespace modulo2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

             var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.ReceiveEndpoint("modulo2", e =>
                {
                    e.Consumer<EventConsumer>();
                });
            });

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await busControl.StartAsync(source.Token);
            try
            {
                Console.WriteLine("Press enter to exit");

                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync();
            }
        }

        class EventConsumer : IConsumer<ValueEntered>
        {
            public async Task Consume(ConsumeContext<ValueEntered> context)
            {
                Console.WriteLine("Modulo 2 - Value: {0}", context.Message.Value);
            }
        }
    }
}

namespace EventContracts
{
    public interface ValueEntered
    {
        string Value { get; }
    }
}