namespace DriversManagement.Domain.Trucks.Mappings;

using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class TruckMapper
{
    public static partial TruckForCreation ToTruckForCreation(this TruckForCreationDto truckForCreationDto);
    public static partial TruckForUpdate ToTruckForUpdate(this TruckForUpdateDto truckForUpdateDto);
    public static partial TruckDto ToTruckDto(this Truck truck);
    public static partial IQueryable<TruckDto> ToTruckDtoQueryable(this IQueryable<Truck> truck);
}