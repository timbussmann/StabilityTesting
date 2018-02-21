using System;
using NServiceBus;

namespace StabilityTesting.Shared
{
    public class PlaceOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}