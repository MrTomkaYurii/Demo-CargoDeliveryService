namespace DriversManagement.SharedTestHelpers.Fakes.Repair;

using AutoBogus;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Dtos;

public sealed class FakeRepairForCreationDto : AutoFaker<RepairForCreationDto>
{
    public FakeRepairForCreationDto()
    {
    }
}