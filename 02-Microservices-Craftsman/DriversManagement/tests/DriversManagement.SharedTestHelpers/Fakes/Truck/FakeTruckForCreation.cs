namespace DriversManagement.SharedTestHelpers.Fakes.Truck;

using AutoBogus;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Models;

public sealed class FakeTruckForCreation : AutoFaker<TruckForCreation>
{
    public FakeTruckForCreation()
    {
    }
}