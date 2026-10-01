namespace DriversBlogManagement.UnitTests.Domain.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class CreatePostAboutDriverTests
{
    private readonly Faker _faker;

    public CreatePostAboutDriverTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_postAboutDriver()
    {
        // Arrange
        var postAboutDriverToCreate = new FakePostAboutDriverForCreation().Generate();
        
        // Act
        var postAboutDriver = PostAboutDriver.Create(postAboutDriverToCreate);

        // Assert
        postAboutDriver.Title.Should().Be(postAboutDriverToCreate.Title);
        postAboutDriver.Content.Should().Be(postAboutDriverToCreate.Content);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var postAboutDriverToCreate = new FakePostAboutDriverForCreation().Generate();
        
        // Act
        var postAboutDriver = PostAboutDriver.Create(postAboutDriverToCreate);

        // Assert
        postAboutDriver.DomainEvents.Count.Should().Be(1);
        postAboutDriver.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(PostAboutDriverCreated));
    }
}