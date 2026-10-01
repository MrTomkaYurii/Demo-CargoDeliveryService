namespace DriversManagement.SharedTestHelpers.Fakes.User;

using AutoBogus;
using DriversManagement.Domain;
using DriversManagement.Domain.Users.Dtos;
using DriversManagement.Domain.Roles;
using DriversManagement.Domain.Users.Models;

public sealed class FakeUserForCreationDto : AutoFaker<UserForCreationDto>
{
    public FakeUserForCreationDto()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}