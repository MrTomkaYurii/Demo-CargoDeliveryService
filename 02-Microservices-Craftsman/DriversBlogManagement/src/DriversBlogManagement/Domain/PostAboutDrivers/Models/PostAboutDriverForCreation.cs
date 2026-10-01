namespace DriversBlogManagement.Domain.PostAboutDrivers.Models;

using Destructurama.Attributed;

public sealed class PostAboutDriverForCreation
{
    public string Title { get; set; }
    public string Content { get; set; }
}
