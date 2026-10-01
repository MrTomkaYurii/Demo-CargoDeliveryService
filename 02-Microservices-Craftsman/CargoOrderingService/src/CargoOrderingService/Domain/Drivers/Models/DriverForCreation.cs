namespace CargoOrderingService.Domain.Drivers.Models;

public sealed record DriverForCreation
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DriverLicenseNumber { get; set; }
    public string Phone { get; set; }
}
