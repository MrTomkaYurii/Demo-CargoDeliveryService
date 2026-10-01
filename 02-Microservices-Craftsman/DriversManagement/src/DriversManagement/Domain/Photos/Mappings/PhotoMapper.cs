namespace DriversManagement.Domain.Photos.Mappings;

using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class PhotoMapper
{
    public static partial PhotoForCreation ToPhotoForCreation(this PhotoForCreationDto photoForCreationDto);
    public static partial PhotoForUpdate ToPhotoForUpdate(this PhotoForUpdateDto photoForUpdateDto);
    public static partial PhotoDto ToPhotoDto(this Photo photo);
    public static partial IQueryable<PhotoDto> ToPhotoDtoQueryable(this IQueryable<Photo> photo);
}