namespace DriversBlogManagement.Domain.Drivers.Dtos;

using Destructurama.Attributed;

public sealed record DriverForCreationDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}
