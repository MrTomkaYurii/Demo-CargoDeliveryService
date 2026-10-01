namespace DriversBlogManagement.Domain.Drivers.Dtos;

using Destructurama.Attributed;

public sealed record DriverDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

}
