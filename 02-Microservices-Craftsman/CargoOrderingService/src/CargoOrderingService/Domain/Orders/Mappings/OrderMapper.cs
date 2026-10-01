namespace CargoOrderingService.Domain.Orders.Mappings;

using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class OrderMapper
{
    public static partial OrderForCreation ToOrderForCreation(this OrderForCreationDto orderForCreationDto);
    public static partial OrderForUpdate ToOrderForUpdate(this OrderForUpdateDto orderForUpdateDto);
    public static partial OrderDto ToOrderDto(this Order order);
    public static partial IQueryable<OrderDto> ToOrderDtoQueryable(this IQueryable<Order> order);
}