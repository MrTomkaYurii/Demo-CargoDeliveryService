using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

namespace DriversManagement.Domain.Drivers.Dtos
{
    public class FullDataAboutDriverAndPostsDto
    {
        public DriverDto Driver { get; set; }
        public List<PostAboutDriverDto> Posts { get; init; }
    }
}
