using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

namespace DriversBlogManagement.Domain.Drivers.Dtos
{
    public sealed class DriverWithPostsDto
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<PostAboutDriverDto> Posts { get; init; }
    }
}
