namespace DriversManagement.SharedTestHelpers.Fakes.Expence;

using AutoBogus;
using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.Models;

public sealed class FakeExpenceForCreation : AutoFaker<ExpenceForCreation>
{
    public FakeExpenceForCreation()
    {
    }
}