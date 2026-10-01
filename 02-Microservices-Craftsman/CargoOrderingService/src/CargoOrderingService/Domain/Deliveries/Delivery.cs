namespace CargoOrderingService.Domain.Deliveries;

using System.ComponentModel.DataAnnotations;
using CargoOrderingService.Domain.Drivers;
using CargoOrderingService.Domain.Orders;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain.Deliveries.Models;
using CargoOrderingService.Domain.Deliveries.DomainEvents;
using CargoOrderingService.Domain.Drivers;
using CargoOrderingService.Domain.Drivers.Models;


public class Delivery : BaseEntity
{
    public string DeliveryDate { get; private set; }

    public string PickupAddress { get; private set; }

    public string DestinationAddress { get; private set; }

    public string PackageDetails { get; private set; }

    public string DeliveryStatus { get; private set; }

    public Order Order { get; }

    public Driver Driver { get; private set; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Delivery Create(DeliveryForCreation deliveryForCreation)
    {
        var newDelivery = new Delivery();

        newDelivery.DeliveryDate = deliveryForCreation.DeliveryDate;
        newDelivery.PickupAddress = deliveryForCreation.PickupAddress;
        newDelivery.DestinationAddress = deliveryForCreation.DestinationAddress;
        newDelivery.PackageDetails = deliveryForCreation.PackageDetails;
        newDelivery.DeliveryStatus = deliveryForCreation.DeliveryStatus;

        newDelivery.QueueDomainEvent(new DeliveryCreated(){ Delivery = newDelivery });
        
        return newDelivery;
    }

    public Delivery Update(DeliveryForUpdate deliveryForUpdate)
    {
        DeliveryDate = deliveryForUpdate.DeliveryDate;
        PickupAddress = deliveryForUpdate.PickupAddress;
        DestinationAddress = deliveryForUpdate.DestinationAddress;
        PackageDetails = deliveryForUpdate.PackageDetails;
        DeliveryStatus = deliveryForUpdate.DeliveryStatus;

        QueueDomainEvent(new DeliveryUpdated(){ Id = Id });
        return this;
    }

    public Delivery SetDriver(Driver driver)
    {
        Driver = driver;
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Delivery() { } // For EF + Mocking
}
