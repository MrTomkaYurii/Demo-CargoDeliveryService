namespace DriversManagement.SharedTestHelpers.Fakes.Expence;

using AutoBogus;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.Dtos;

public sealed class FakeExpenceForUpdateDto : AutoFaker<ExpenceForUpdateDto>
{
    public FakeExpenceForUpdateDto()
    {
    }
}