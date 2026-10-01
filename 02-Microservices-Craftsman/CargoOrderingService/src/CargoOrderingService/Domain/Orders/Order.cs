namespace CargoOrderingService.Domain.Orders;

using System.ComponentModel.DataAnnotations;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Models;
using CargoOrderingService.Domain.Drivers;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain.Orders.Models;
using CargoOrderingService.Domain.Orders.DomainEvents;
using CargoOrderingService.Domain.Drivers;
using CargoOrderingService.Domain.Drivers.Models;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Models;


public class Order : BaseEntity
{
    public string OrderNumber { get; private set; }

    public string CustomerName { get; private set; }

    public string DeliveryDate { get; private set; }

    public string TotalAmount { get; private set; }

    public string Status { get; private set; }

    public Driver Driver { get; private set; }

    public Delivery Delivery { get; private set; } = Delivery.Create(new DeliveryForCreation());

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Order Create(OrderForCreation orderForCreation)
    {
        var newOrder = new Order();

        newOrder.OrderNumber = orderForCreation.OrderNumber;
        newOrder.CustomerName = orderForCreation.CustomerName;
        newOrder.DeliveryDate = orderForCreation.DeliveryDate;
        newOrder.TotalAmount = orderForCreation.TotalAmount;
        newOrder.Status = orderForCreation.Status;

        newOrder.QueueDomainEvent(new OrderCreated(){ Order = newOrder });
        
        return newOrder;
    }

    public Order Update(OrderForUpdate orderForUpdate)
    {
        OrderNumber = orderForUpdate.OrderNumber;
        CustomerName = orderForUpdate.CustomerName;
        DeliveryDate = orderForUpdate.DeliveryDate;
        TotalAmount = orderForUpdate.TotalAmount;
        Status = orderForUpdate.Status;

        QueueDomainEvent(new OrderUpdated(){ Id = Id });
        return this;
    }

    public Order SetDriver(Driver driver)
    {
        Driver = driver;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Order() { } // For EF + Mocking
}
