namespace DriversBlogManagement.Domain.BlogUsers.Mappings;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class BlogUserMapper
{
    public static partial BlogUserForCreation ToBlogUserForCreation(this BlogUserForCreationDto blogUserForCreationDto);
    public static partial BlogUserForUpdate ToBlogUserForUpdate(this BlogUserForUpdateDto blogUserForUpdateDto);
    public static partial BlogUserDto ToBlogUserDto(this BlogUser blogUser);
    public static partial IQueryable<BlogUserDto> ToBlogUserDtoQueryable(this IQueryable<BlogUser> blogUser);
}