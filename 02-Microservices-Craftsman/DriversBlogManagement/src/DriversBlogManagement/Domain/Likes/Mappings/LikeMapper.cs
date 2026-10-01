namespace DriversBlogManagement.Domain.Likes.Mappings;

using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.Domain.Likes.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class LikeMapper
{
    public static partial LikeForCreation ToLikeForCreation(this LikeForCreationDto likeForCreationDto);
    public static partial LikeForUpdate ToLikeForUpdate(this LikeForUpdateDto likeForUpdateDto);
    public static partial LikeDto ToLikeDto(this Like like);
    public static partial IQueryable<LikeDto> ToLikeDtoQueryable(this IQueryable<Like> like);
}