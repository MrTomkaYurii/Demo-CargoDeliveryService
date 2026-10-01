namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateBlogUserCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_bloguser_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUser = new FakeBlogUserBuilder().Build();
        await testingServiceScope.InsertAsync(blogUser);
        var updatedBlogUserDto = new FakeBlogUserForUpdateDto().Generate();

        // Act
        var command = new UpdateBlogUser.Command(blogUser.Id, updatedBlogUserDto);
        await testingServiceScope.SendAsync(command);
        var updatedBlogUser = await testingServiceScope
            .ExecuteDbContextAsync(db => db.BlogUsers
                .FirstOrDefaultAsync(b => b.Id == blogUser.Id));

        // Assert
        updatedBlogUser.UserName.Should().Be(updatedBlogUserDto.UserName);
        updatedBlogUser.Email.Should().Be(updatedBlogUserDto.Email);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateUser);
        var blogUserOne = new FakeBlogUserForUpdateDto();

        // Act
        var command = new UpdateBlogUser.Command(Guid.NewGuid(), blogUserOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}