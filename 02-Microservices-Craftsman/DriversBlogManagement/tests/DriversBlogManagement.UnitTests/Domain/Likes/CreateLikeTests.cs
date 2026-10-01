namespace DriversBlogManagement.UnitTests.Domain.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Domain.Likes.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class CreateLikeTests
{
    private readonly Faker _faker;

    public CreateLikeTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_like()
    {
        // Arrange
        var likeToCreate = new FakeLikeForCreation().Generate();
        
        // Act
        var like = Like.Create(likeToCreate);

        // Assert
        like.Info.Should().Be(likeToCreate.Info);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var likeToCreate = new FakeLikeForCreation().Generate();
        
        // Act
        var like = Like.Create(likeToCreate);

        // Assert
        like.DomainEvents.Count.Should().Be(1);
        like.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(LikeCreated));
    }
}