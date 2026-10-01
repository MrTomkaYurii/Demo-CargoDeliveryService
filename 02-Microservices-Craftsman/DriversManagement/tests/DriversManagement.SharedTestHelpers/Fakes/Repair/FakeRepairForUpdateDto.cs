namespace DriversManagement.SharedTestHelpers.Fakes.Repair;

using AutoBogus;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Dtos;

public sealed class FakeRepairForUpdateDto : AutoFaker<RepairForUpdateDto>
{
    public FakeRepairForUpdateDto()
    {
    }
}