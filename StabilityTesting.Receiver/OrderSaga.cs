using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence.Sql;
using StabilityTesting.Shared;

namespace StabilityTesting.Receiver
{
    public class OrderSaga : SqlSaga<OrderSagaData>, IAmStartedByMessages<PlaceOrder>, IHandleTimeouts<CompleteOrderTimeout>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderSaga>();

        protected override string CorrelationPropertyName => nameof(OrderSagaData.OrderId);

        protected override void ConfigureMapping(IMessagePropertyMapper mapper)
        {
            mapper.ConfigureMapping<PlaceOrder>(m => m.OrderId);
        }

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Log.Info("Start OrderSaga for order " + message.OrderId);
            return RequestTimeout<CompleteOrderTimeout>(context, TimeSpan.FromMinutes(1));
        }

        public async Task Timeout(CompleteOrderTimeout state, IMessageHandlerContext context)
        {
            Log.Info("Complete Order " + Data.OrderId);
            await context.Publish(new OrderCompleted {OrderId = Data.OrderId});
            MarkAsComplete();
        }
    }

    public class CompleteOrderTimeout
    {
    }

    public class OrderSagaData : ContainSagaData
    {
        public Guid OrderId { get; set; }
    }
}