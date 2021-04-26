using System;
using System.Threading;
using System.Threading.Tasks;
using EventContracts;
using MassTransit;
using GreenPipes;

namespace modulo4
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

             var busControl = Bus.Factory.CreateUsingInMemory( cfg=>
             {
                cfg.TransportConcurrencyLimit = 100;
                cfg.ReceiveEndpoint("Consumer1", e =>
                {
                    e.UseMessageRetry(r => r.Immediate( 5));
                    e.Consumer<Consumer1>();
                });
                cfg.ReceiveEndpoint("Consumer2", e =>
                {
                    e.UseMessageRetry(r => r.Immediate( 5));
                    e.Consumer<Consumer2>();
                });
             });

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await busControl.StartAsync(source.Token);

            try
            {
                while (true)
                {
                    string value = await Task.Run(() =>
                    {
                        Console.WriteLine("Enter message (or quit to exit)");
                        Console.Write("> ");
                        return Console.ReadLine();
                    });

                    if("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
                        break;

                    await busControl.Publish<ValueEntered>(new
                    {
                        Value = value
                    },
                    context => {
                        context.Durable = true;
                        context.TimeToLive = TimeSpan.FromSeconds(10);
                    });
                }
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }

    public class Consumer1:IConsumer<ValueEntered>
    {
        static int counter =0;
            public async Task Consume(ConsumeContext<ValueEntered> context)
            {
                counter++;
                if(counter %2 ==0)
                    throw new InvalidOperationException();
                Console.WriteLine("Consumer1 - Value: {0}", context.Message.Value);
            }
    }
    public class Consumer2:IConsumer<ValueEntered>
    {
        static int counter =0;
            public async Task Consume(ConsumeContext<ValueEntered> context)
            {
                counter++;
                if(counter %2 ==1)
                    throw new InvalidOperationException();
                Console.WriteLine("Consumer2 - Value: {0}", context.Message.Value);
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

