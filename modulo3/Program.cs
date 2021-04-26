using System;
using System.Threading;
using System.Threading.Tasks;
using EventContracts;
using GreenPipes;
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
                 cfg.Host("localhost","/", h=>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("transient", e =>
                {
                    e.UseMessageRetry(r => r.Incremental(5,TimeSpan.FromSeconds(2),TimeSpan.FromSeconds(3)));
                    e.Consumer<EventConsumer>();
                });
                cfg.UseRetry( e => 
                {
                    e.Immediate(5);
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

        class EventConsumer : IConsumer<IValueEntered>
        {
            static int counter =0;
            public async Task Consume(ConsumeContext<IValueEntered> context)
            {
                counter++;
                if(counter %2 ==0)
                    throw new InvalidOperationException();
                Console.WriteLine("Transient - Value: {0}", context.Message.Value);
            }
        }
    }
}

namespace EventContracts
{
    public interface IValueEntered
    {
        string Value { get; }
    }
}