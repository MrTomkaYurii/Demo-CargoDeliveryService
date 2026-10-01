namespace DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

using Destructurama.Attributed;

public sealed record PostAboutDriverDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}
