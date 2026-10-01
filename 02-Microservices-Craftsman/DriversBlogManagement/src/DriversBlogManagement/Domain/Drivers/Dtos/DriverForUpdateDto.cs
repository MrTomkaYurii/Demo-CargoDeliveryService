namespace DriversBlogManagement.Domain.Drivers.Dtos;

using Destructurama.Attributed;

public sealed record DriverForUpdateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}
