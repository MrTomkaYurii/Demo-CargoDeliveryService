namespace DriversManagement.SharedTestHelpers.Fakes.Photo;

using AutoBogus;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Models;

public sealed class FakePhotoForCreation : AutoFaker<PhotoForCreation>
{
    public FakePhotoForCreation()
    {
    }
}