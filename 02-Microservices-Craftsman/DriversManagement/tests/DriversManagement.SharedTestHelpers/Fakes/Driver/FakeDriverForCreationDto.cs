namespace DriversManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Dtos;

public sealed class FakeDriverForCreationDto : AutoFaker<DriverForCreationDto>
{
    public FakeDriverForCreationDto()
    {
    }
}