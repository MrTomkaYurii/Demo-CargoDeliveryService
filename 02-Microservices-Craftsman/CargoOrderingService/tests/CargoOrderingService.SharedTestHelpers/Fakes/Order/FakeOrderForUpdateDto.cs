namespace CargoOrderingService.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Dtos;

public sealed class FakeOrderForUpdateDto : AutoFaker<OrderForUpdateDto>
{
    public FakeOrderForUpdateDto()
    {
    }
}