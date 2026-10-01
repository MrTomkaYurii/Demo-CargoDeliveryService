namespace DriversBlogManagement.SharedTestHelpers.Fakes.Like;

using AutoBogus;
using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Domain.Likes.Dtos;

public sealed class FakeLikeForUpdateDto : AutoFaker<LikeForUpdateDto>
{
    public FakeLikeForUpdateDto()
    {
    }
}