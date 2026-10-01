namespace DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

using Destructurama.Attributed;

public sealed record PostAboutDriverForCreationDto
{
    public string Title { get; set; }
    public string Content { get; set; }
}
