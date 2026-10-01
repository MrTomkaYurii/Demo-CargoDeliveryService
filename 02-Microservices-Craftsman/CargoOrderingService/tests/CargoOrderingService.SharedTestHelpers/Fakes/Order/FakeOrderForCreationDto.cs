namespace CargoOrderingService.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Dtos;

public sealed class FakeOrderForCreationDto : AutoFaker<OrderForCreationDto>
{
    public FakeOrderForCreationDto()
    {
    }
}