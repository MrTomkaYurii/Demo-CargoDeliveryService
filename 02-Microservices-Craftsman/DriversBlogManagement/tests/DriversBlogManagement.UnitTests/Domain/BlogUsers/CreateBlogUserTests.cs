namespace DriversBlogManagement.UnitTests.Domain.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class CreateBlogUserTests
{
    private readonly Faker _faker;

    public CreateBlogUserTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_blogUser()
    {
        // Arrange
        var blogUserToCreate = new FakeBlogUserForCreation().Generate();
        
        // Act
        var blogUser = BlogUser.Create(blogUserToCreate);

        // Assert
        blogUser.UserName.Should().Be(blogUserToCreate.UserName);
        blogUser.Email.Should().Be(blogUserToCreate.Email);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var blogUserToCreate = new FakeBlogUserForCreation().Generate();
        
        // Act
        var blogUser = BlogUser.Create(blogUserToCreate);

        // Assert
        blogUser.DomainEvents.Count.Should().Be(1);
        blogUser.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(BlogUserCreated));
    }
}