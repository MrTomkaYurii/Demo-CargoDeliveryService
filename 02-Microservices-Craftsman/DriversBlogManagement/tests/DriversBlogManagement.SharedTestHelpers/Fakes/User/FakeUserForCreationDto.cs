namespace DriversBlogManagement.SharedTestHelpers.Fakes.User;

using AutoBogus;
using DriversBlogManagement.Domain;
using DriversBlogManagement.Domain.Users.Dtos;
using DriversBlogManagement.Domain.Roles;
using DriversBlogManagement.Domain.Users.Models;

public sealed class FakeUserForCreationDto : AutoFaker<UserForCreationDto>
{
    public FakeUserForCreationDto()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}