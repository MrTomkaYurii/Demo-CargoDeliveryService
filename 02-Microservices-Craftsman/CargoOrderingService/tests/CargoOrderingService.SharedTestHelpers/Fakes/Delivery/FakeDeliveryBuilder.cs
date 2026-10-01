namespace CargoOrderingService.SharedTestHelpers.Fakes.Delivery;

using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Models;

public class FakeDeliveryBuilder
{
    private DeliveryForCreation _creationData = new FakeDeliveryForCreation().Generate();

    public FakeDeliveryBuilder WithModel(DeliveryForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeDeliveryBuilder WithDeliveryDate(string deliveryDate)
    {
        _creationData.DeliveryDate = deliveryDate;
        return this;
    }
    
    public FakeDeliveryBuilder WithPickupAddress(string pickupAddress)
    {
        _creationData.PickupAddress = pickupAddress;
        return this;
    }
    
    public FakeDeliveryBuilder WithDestinationAddress(string destinationAddress)
    {
        _creationData.DestinationAddress = destinationAddress;
        return this;
    }
    
    public FakeDeliveryBuilder WithPackageDetails(string packageDetails)
    {
        _creationData.PackageDetails = packageDetails;
        return this;
    }
    
    public FakeDeliveryBuilder WithDeliveryStatus(string deliveryStatus)
    {
        _creationData.DeliveryStatus = deliveryStatus;
        return this;
    }
    
    public Delivery Build()
    {
        var result = Delivery.Create(_creationData);
        return result;
    }
}