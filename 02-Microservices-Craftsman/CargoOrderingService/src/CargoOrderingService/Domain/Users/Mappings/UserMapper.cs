namespace CargoOrderingService.Domain.Users.Mappings;

using CargoOrderingService.Domain.Users.Dtos;
using CargoOrderingService.Domain.Users.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class UserMapper
{
    public static partial UserForCreation ToUserForCreation(this UserForCreationDto userForCreationDto);
    public static partial UserForUpdate ToUserForUpdate(this UserForUpdateDto userForUpdateDto);
    public static partial UserDto ToUserDto(this User user);
    public static partial IQueryable<UserDto> ToUserDtoQueryable(this IQueryable<User> user);
}