namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteBlogUserCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_bloguser_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUser = new FakeBlogUserBuilder().Build();
        await testingServiceScope.InsertAsync(blogUser);

        // Act
        var command = new DeleteBlogUser.Command(blogUser.Id);
        await testingServiceScope.SendAsync(command);
        var blogUserResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.BlogUsers
                .CountAsync(b => b.Id == blogUser.Id));

        // Assert
        blogUserResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_bloguser_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteBlogUser.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_bloguser_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUser = new FakeBlogUserBuilder().Build();
        await testingServiceScope.InsertAsync(blogUser);

        // Act
        var command = new DeleteBlogUser.Command(blogUser.Id);
        await testingServiceScope.SendAsync(command);
        var deletedBlogUser = await testingServiceScope.ExecuteDbContextAsync(db => db.BlogUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == blogUser.Id));

        // Assert
        deletedBlogUser?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteUser);

        // Act
        var command = new DeleteBlogUser.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}