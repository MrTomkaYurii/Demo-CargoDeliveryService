namespace DriversManagement.SharedTestHelpers.Fakes.Expence;

using AutoBogus;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.Dtos;

public sealed class FakeExpenceForCreationDto : AutoFaker<ExpenceForCreationDto>
{
    public FakeExpenceForCreationDto()
    {
    }
}