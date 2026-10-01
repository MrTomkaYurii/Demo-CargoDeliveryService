namespace DriversBlogManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.Models;

public sealed class FakeDriverForCreation : AutoFaker<DriverForCreation>
{
    public FakeDriverForCreation()
    {
    }
}