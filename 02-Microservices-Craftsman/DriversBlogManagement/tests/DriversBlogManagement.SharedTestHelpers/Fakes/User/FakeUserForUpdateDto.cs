namespace DriversBlogManagement.SharedTestHelpers.Fakes.User;

using AutoBogus;
using DriversBlogManagement.Domain;
using DriversBlogManagement.Domain.Users.Dtos;
using DriversBlogManagement.Domain.Roles;
using DriversBlogManagement.Domain.Users.Models;

public sealed class FakeUserForUpdateDto : AutoFaker<UserForUpdateDto>
{
    public FakeUserForUpdateDto()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}