using System;
using NServiceBus;

namespace StabilityTesting.Shared
{
    public class OrderCompleted : IEvent
    {
        public Guid OrderId { get; set; }
    }
}