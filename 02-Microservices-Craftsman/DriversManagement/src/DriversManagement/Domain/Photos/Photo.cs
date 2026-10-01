namespace DriversManagement.Domain.Photos;

using System.ComponentModel.DataAnnotations;
using DriversManagement.Domain.Drivers;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using DriversManagement.Exceptions;
using DriversManagement.Domain.Photos.Models;
using DriversManagement.Domain.Photos.DomainEvents;


public class Photo : BaseEntity
{
    public string PhotoData { get; private set; }

    public string ContentType { get; private set; }

    public string FileName { get; private set; }

    public string FileSizels { get; private set; }

    public Driver Driver { get; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Photo Create(PhotoForCreation photoForCreation)
    {
        var newPhoto = new Photo();

        newPhoto.PhotoData = photoForCreation.PhotoData;
        newPhoto.ContentType = photoForCreation.ContentType;
        newPhoto.FileName = photoForCreation.FileName;
        newPhoto.FileSizels = photoForCreation.FileSizels;

        newPhoto.QueueDomainEvent(new PhotoCreated(){ Photo = newPhoto });
        
        return newPhoto;
    }

    public Photo Update(PhotoForUpdate photoForUpdate)
    {
        PhotoData = photoForUpdate.PhotoData;
        ContentType = photoForUpdate.ContentType;
        FileName = photoForUpdate.FileName;
        FileSizels = photoForUpdate.FileSizels;

        QueueDomainEvent(new PhotoUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Photo() { } // For EF + Mocking
}
