namespace DriversBlogManagement.UnitTests.Domain.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class UpdateBlogUserTests
{
    private readonly Faker _faker;

    public UpdateBlogUserTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_blogUser()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();
        var updatedBlogUser = new FakeBlogUserForUpdate().Generate();
        
        // Act
        blogUser.Update(updatedBlogUser);

        // Assert
        blogUser.UserName.Should().Be(updatedBlogUser.UserName);
        blogUser.Email.Should().Be(updatedBlogUser.Email);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();
        var updatedBlogUser = new FakeBlogUserForUpdate().Generate();
        blogUser.DomainEvents.Clear();
        
        // Act
        blogUser.Update(updatedBlogUser);

        // Assert
        blogUser.DomainEvents.Count.Should().Be(1);
        blogUser.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(BlogUserUpdated));
    }
}