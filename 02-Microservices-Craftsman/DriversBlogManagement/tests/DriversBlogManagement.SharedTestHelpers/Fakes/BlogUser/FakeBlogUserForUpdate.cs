namespace DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;

using AutoBogus;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.Models;

public sealed class FakeBlogUserForUpdate : AutoFaker<BlogUserForUpdate>
{
    public FakeBlogUserForUpdate()
    {
    }
}