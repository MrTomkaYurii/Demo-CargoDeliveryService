namespace DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;

using AutoBogus;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.Models;

public sealed class FakeBlogUserForCreation : AutoFaker<BlogUserForCreation>
{
    public FakeBlogUserForCreation()
    {
    }
}