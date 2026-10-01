namespace DriversManagement.UnitTests.Domain.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdatePhotoTests
{
    private readonly Faker _faker;

    public UpdatePhotoTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_photo()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        var updatedPhoto = new FakePhotoForUpdate().Generate();
        
        // Act
        photo.Update(updatedPhoto);

        // Assert
        photo.PhotoData.Should().Be(updatedPhoto.PhotoData);
        photo.ContentType.Should().Be(updatedPhoto.ContentType);
        photo.FileName.Should().Be(updatedPhoto.FileName);
        photo.FileSizels.Should().Be(updatedPhoto.FileSizels);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        var updatedPhoto = new FakePhotoForUpdate().Generate();
        photo.DomainEvents.Clear();
        
        // Act
        photo.Update(updatedPhoto);

        // Assert
        photo.DomainEvents.Count.Should().Be(1);
        photo.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(PhotoUpdated));
    }
}