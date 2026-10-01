namespace DriversBlogManagement.UnitTests.Domain.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Domain.Likes.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class UpdateLikeTests
{
    private readonly Faker _faker;

    public UpdateLikeTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_like()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        var updatedLike = new FakeLikeForUpdate().Generate();
        
        // Act
        like.Update(updatedLike);

        // Assert
        like.Info.Should().Be(updatedLike.Info);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        var updatedLike = new FakeLikeForUpdate().Generate();
        like.DomainEvents.Clear();
        
        // Act
        like.Update(updatedLike);

        // Assert
        like.DomainEvents.Count.Should().Be(1);
        like.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(LikeUpdated));
    }
}