namespace DriversManagement.SharedTestHelpers.Fakes.Truck;

using AutoBogus;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Dtos;

public sealed class FakeTruckForUpdateDto : AutoFaker<TruckForUpdateDto>
{
    public FakeTruckForUpdateDto()
    {
    }
}