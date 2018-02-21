using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using StabilityTesting.Shared;

namespace StabilityTesting.Sender
{
    public class OrderCompletedHandler : IHandleMessages<OrderCompleted>
    {
        static readonly ILog Log = LogManager.GetLogger<OrderCompletedHandler>();

        public Task Handle(OrderCompleted message, IMessageHandlerContext context)
        {
            Log.Info("Received OrderCompleted for order " + message.OrderId);
            return Task.CompletedTask;
        }
    }
}