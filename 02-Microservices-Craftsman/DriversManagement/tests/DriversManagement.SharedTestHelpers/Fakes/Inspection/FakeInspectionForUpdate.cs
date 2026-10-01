namespace DriversManagement.SharedTestHelpers.Fakes.Inspection;

using AutoBogus;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.Models;

public sealed class FakeInspectionForUpdate : AutoFaker<InspectionForUpdate>
{
    public FakeInspectionForUpdate()
    {
    }
}