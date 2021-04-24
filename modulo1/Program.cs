using System;
using System.Threading;
using System.Threading.Tasks;
using EventContracts;
using MassTransit;

namespace modulo1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

             var busControl = Bus.Factory.CreateUsingRabbitMq( cfg=>
             {
                cfg.Host("localhost","/", h=>
                {
                    h.Username("guest");
                    h.Password("guest");
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
}

namespace EventContracts
{
    public interface ValueEntered
    {
        string Value { get; }
    }
}
