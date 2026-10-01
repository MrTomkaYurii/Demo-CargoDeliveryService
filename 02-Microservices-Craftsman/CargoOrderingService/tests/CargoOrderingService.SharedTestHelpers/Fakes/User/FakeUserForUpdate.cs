namespace CargoOrderingService.SharedTestHelpers.Fakes.User;

using AutoBogus;
using CargoOrderingService.Domain;
using CargoOrderingService.Domain.Users.Dtos;
using CargoOrderingService.Domain.Roles;
using CargoOrderingService.Domain.Users.Models;

public sealed class FakeUserForUpdate : AutoFaker<UserForUpdate>
{
    public FakeUserForUpdate()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}