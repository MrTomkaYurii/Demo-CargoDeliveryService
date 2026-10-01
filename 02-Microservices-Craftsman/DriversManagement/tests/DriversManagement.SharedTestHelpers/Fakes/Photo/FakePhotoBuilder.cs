namespace DriversManagement.SharedTestHelpers.Fakes.Photo;

using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Models;

public class FakePhotoBuilder
{
    private PhotoForCreation _creationData = new FakePhotoForCreation().Generate();

    public FakePhotoBuilder WithModel(PhotoForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakePhotoBuilder WithPhotoData(string photoData)
    {
        _creationData.PhotoData = photoData;
        return this;
    }
    
    public FakePhotoBuilder WithContentType(string contentType)
    {
        _creationData.ContentType = contentType;
        return this;
    }
    
    public FakePhotoBuilder WithFileName(string fileName)
    {
        _creationData.FileName = fileName;
        return this;
    }
    
    public FakePhotoBuilder WithFileSizels(string fileSizels)
    {
        _creationData.FileSizels = fileSizels;
        return this;
    }
    
    public Photo Build()
    {
        var result = Photo.Create(_creationData);
        return result;
    }
}