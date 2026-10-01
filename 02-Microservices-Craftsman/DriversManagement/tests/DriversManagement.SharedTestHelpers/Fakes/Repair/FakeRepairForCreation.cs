namespace DriversManagement.SharedTestHelpers.Fakes.Repair;

using AutoBogus;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Models;

public sealed class FakeRepairForCreation : AutoFaker<RepairForCreation>
{
    public FakeRepairForCreation()
    {
    }
}