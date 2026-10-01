namespace DriversManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Models;

public sealed class FakeDriverForCreation : AutoFaker<DriverForCreation>
{
    public FakeDriverForCreation()
    {
    }
}