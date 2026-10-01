namespace CargoOrderingService.SharedTestHelpers.Fakes.Order;

using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Models;

public class FakeOrderBuilder
{
    private OrderForCreation _creationData = new FakeOrderForCreation().Generate();

    public FakeOrderBuilder WithModel(OrderForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeOrderBuilder WithOrderNumber(string orderNumber)
    {
        _creationData.OrderNumber = orderNumber;
        return this;
    }
    
    public FakeOrderBuilder WithCustomerName(string customerName)
    {
        _creationData.CustomerName = customerName;
        return this;
    }
    
    public FakeOrderBuilder WithDeliveryDate(string deliveryDate)
    {
        _creationData.DeliveryDate = deliveryDate;
        return this;
    }
    
    public FakeOrderBuilder WithTotalAmount(string totalAmount)
    {
        _creationData.TotalAmount = totalAmount;
        return this;
    }
    
    public FakeOrderBuilder WithStatus(string status)
    {
        _creationData.Status = status;
        return this;
    }
    
    public Order Build()
    {
        var result = Order.Create(_creationData);
        return result;
    }
}