namespace DriversManagement.UnitTests.Domain.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreatePhotoTests
{
    private readonly Faker _faker;

    public CreatePhotoTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_photo()
    {
        // Arrange
        var photoToCreate = new FakePhotoForCreation().Generate();
        
        // Act
        var photo = Photo.Create(photoToCreate);

        // Assert
        photo.PhotoData.Should().Be(photoToCreate.PhotoData);
        photo.ContentType.Should().Be(photoToCreate.ContentType);
        photo.FileName.Should().Be(photoToCreate.FileName);
        photo.FileSizels.Should().Be(photoToCreate.FileSizels);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var photoToCreate = new FakePhotoForCreation().Generate();
        
        // Act
        var photo = Photo.Create(photoToCreate);

        // Assert
        photo.DomainEvents.Count.Should().Be(1);
        photo.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(PhotoCreated));
    }
}