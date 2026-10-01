using System;

namespace SharedKernel.Messages
{
    public interface IOrderCreated
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmount { get; set; }
    }

    public class OrderCreatedMessage : IOrderCreated
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmount { get; set; }
    }

    public interface IOrderAssignedToDriver
    {
        public Guid OrderId { get; set; }
        public Guid DriverId { get; set; }
        public DateTime AssignedAtUtc { get; set; }
    }

    public class OrderAssignedToDriverMessage : IOrderAssignedToDriver
    {
        public Guid OrderId { get; set; }
        public Guid DriverId { get; set; }
        public DateTime AssignedAtUtc { get; set; }
    }
}
