namespace DriversBlogManagement.UnitTests.Domain.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class UpdatePostAboutDriverTests
{
    private readonly Faker _faker;

    public UpdatePostAboutDriverTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_postAboutDriver()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        var updatedPostAboutDriver = new FakePostAboutDriverForUpdate().Generate();
        
        // Act
        postAboutDriver.Update(updatedPostAboutDriver);

        // Assert
        postAboutDriver.Title.Should().Be(updatedPostAboutDriver.Title);
        postAboutDriver.Content.Should().Be(updatedPostAboutDriver.Content);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        var updatedPostAboutDriver = new FakePostAboutDriverForUpdate().Generate();
        postAboutDriver.DomainEvents.Clear();
        
        // Act
        postAboutDriver.Update(updatedPostAboutDriver);

        // Assert
        postAboutDriver.DomainEvents.Count.Should().Be(1);
        postAboutDriver.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(PostAboutDriverUpdated));
    }
}