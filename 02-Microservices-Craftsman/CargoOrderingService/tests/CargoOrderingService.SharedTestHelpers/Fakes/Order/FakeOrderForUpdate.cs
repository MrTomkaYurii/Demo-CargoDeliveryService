namespace CargoOrderingService.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Models;

public sealed class FakeOrderForUpdate : AutoFaker<OrderForUpdate>
{
    public FakeOrderForUpdate()
    {
    }
}