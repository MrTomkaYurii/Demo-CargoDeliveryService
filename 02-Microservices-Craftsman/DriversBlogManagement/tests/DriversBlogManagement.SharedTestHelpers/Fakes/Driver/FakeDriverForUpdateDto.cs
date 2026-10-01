namespace DriversBlogManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.Dtos;

public sealed class FakeDriverForUpdateDto : AutoFaker<DriverForUpdateDto>
{
    public FakeDriverForUpdateDto()
    {
    }
}