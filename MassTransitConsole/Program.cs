using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace MassTransitConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateBus();
            Console.ReadKey();
        }

        static async void CreateBus()
        {
            var bus = Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.ReceiveEndpoint("queue", op =>
                {
                    op.Handler<Message>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                    });
                });
            });
            await bus.StartAsync();
            try
            {
                var index = 0;
                while (true)
                {
                    var entity = new Message()
                    {
                        Text = $"{DateTime.Now} => message {index++}"
                    };

                    //If using RabbitMq
                    //var channel = "queue";
                    // var endpointAddr = new Uri("rabbitmq://localhost/" + channel);
                    // var endpoint = await bus.GetSendEndpoint(endpointAddr);
                    // await endpoint.Send(entity);
                    
                    object p = bus.Publish(entity);

                    await Task.Run(() => Thread.Sleep(1000));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await bus.StopAsync();
            }
        }
    }
}
