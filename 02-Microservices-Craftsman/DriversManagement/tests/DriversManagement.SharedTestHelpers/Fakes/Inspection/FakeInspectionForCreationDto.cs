namespace DriversManagement.SharedTestHelpers.Fakes.Inspection;

using AutoBogus;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.Dtos;

public sealed class FakeInspectionForCreationDto : AutoFaker<InspectionForCreationDto>
{
    public FakeInspectionForCreationDto()
    {
    }
}