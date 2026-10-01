namespace DriversBlogManagement.Domain.PostAboutDrivers.Mappings;

using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.Domain.PostAboutDrivers.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class PostAboutDriverMapper
{
    public static partial PostAboutDriverForCreation ToPostAboutDriverForCreation(this PostAboutDriverForCreationDto postAboutDriverForCreationDto);
    public static partial PostAboutDriverForUpdate ToPostAboutDriverForUpdate(this PostAboutDriverForUpdateDto postAboutDriverForUpdateDto);
    public static partial PostAboutDriverDto ToPostAboutDriverDto(this PostAboutDriver postAboutDriver);
    public static partial IQueryable<PostAboutDriverDto> ToPostAboutDriverDtoQueryable(this IQueryable<PostAboutDriver> postAboutDriver);
}