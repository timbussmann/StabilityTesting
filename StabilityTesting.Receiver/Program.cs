﻿using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.Sql;

namespace StabilityTesting.Receiver
{
    class Program
    {
        //private const string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=StabilityTesting;Integrated Security=True;";

        static async Task Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("StabilityTesting_SQLConnectionString");
            var endpointConfiguration = new EndpointConfiguration("StabilityTesting.Receiver");

            var transportConfiguration = endpointConfiguration.UseTransport<SqlServerTransport>();
            transportConfiguration.ConnectionString(connectionString);
            transportConfiguration.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

            var persistenceConfiguration = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistenceConfiguration.SqlDialect<SqlDialect.MsSqlServer>();
            persistenceConfiguration.SubscriptionSettings().CacheFor(TimeSpan.FromMinutes(1));
            persistenceConfiguration.ConnectionBuilder(() => new SqlConnection(connectionString));

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine("Press [Esc] to exit.");
            ConsoleKeyInfo input;
            do
            {
                input = Console.ReadKey();
            } while (input.Key != ConsoleKey.Escape);

            await endpointInstance.Stop();
        }
    }
}
