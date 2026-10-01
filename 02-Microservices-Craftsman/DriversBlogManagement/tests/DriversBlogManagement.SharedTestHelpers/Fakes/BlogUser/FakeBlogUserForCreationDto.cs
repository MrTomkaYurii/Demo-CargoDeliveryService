namespace DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;

using AutoBogus;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.Dtos;

public sealed class FakeBlogUserForCreationDto : AutoFaker<BlogUserForCreationDto>
{
    public FakeBlogUserForCreationDto()
    {
    }
}