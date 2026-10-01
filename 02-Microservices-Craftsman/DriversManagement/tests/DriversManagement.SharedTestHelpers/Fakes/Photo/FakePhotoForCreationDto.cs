namespace DriversManagement.SharedTestHelpers.Fakes.Photo;

using AutoBogus;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Dtos;

public sealed class FakePhotoForCreationDto : AutoFaker<PhotoForCreationDto>
{
    public FakePhotoForCreationDto()
    {
    }
}