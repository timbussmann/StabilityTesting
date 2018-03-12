using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence.Sql;
using StabilityTesting.Shared;

namespace StabilityTesting.Sender
{
    class Program
    {
        //private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=StabilityTesting;Integrated Security=True;";

        static async Task Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("StabilityTesting_RabbitMQConnectionString");

            var endpointConfiguration = new EndpointConfiguration("StabilityTesting.Sender");

            var transportConfiguration = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transportConfiguration.ConnectionString(connectionString);
            var routingSettings = transportConfiguration.Routing();
            routingSettings.RouteToEndpoint(typeof(PlaceOrder), "StabilityTesting.Receiver");

            var persistenceConfiguration = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistenceConfiguration.SqlDialect<SqlDialect.MsSqlServer>();
            persistenceConfiguration.SubscriptionSettings().CacheFor(TimeSpan.FromMinutes(1));
            persistenceConfiguration.ConnectionBuilder(() => new SqlConnection(connectionString));

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            var log = LogManager.GetLogger("StabilityTesting.Sender");

            bool running = true;
            var senderTask = Task.Run(async () =>
            {
                while (running)
                {
                    var placeOrder = new PlaceOrder { OrderId = Guid.NewGuid() };
                    try
                    {
                        
                        await endpointInstance.Send(placeOrder);
                        log.Info("Placing order " + placeOrder.OrderId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception sending a PlaceOrder command: " + e);
                        log.Error("Failed to send PlaceOrder for " + placeOrder.OrderId, e);
                    }
                    await Task.Delay(200);
                }
            });

            Console.WriteLine("Press [Esc] to exit.");
            ConsoleKeyInfo input;
            do
            {
                input = Console.ReadKey();
            } while (input.Key != ConsoleKey.Escape);

            running = false;
            await senderTask;
            await endpointInstance.Stop();
        }
    }
}
