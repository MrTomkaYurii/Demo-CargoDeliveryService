namespace DriversManagement.SharedTestHelpers.Fakes.Photo;

using AutoBogus;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Dtos;

public sealed class FakePhotoForUpdateDto : AutoFaker<PhotoForUpdateDto>
{
    public FakePhotoForUpdateDto()
    {
    }
}