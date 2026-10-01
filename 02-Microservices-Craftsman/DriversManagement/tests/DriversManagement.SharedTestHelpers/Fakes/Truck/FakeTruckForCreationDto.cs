namespace DriversManagement.SharedTestHelpers.Fakes.Truck;

using AutoBogus;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Dtos;

public sealed class FakeTruckForCreationDto : AutoFaker<TruckForCreationDto>
{
    public FakeTruckForCreationDto()
    {
    }
}