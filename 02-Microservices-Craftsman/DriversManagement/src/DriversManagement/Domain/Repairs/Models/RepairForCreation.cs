namespace DriversManagement.Domain.Repairs.Models;

using Destructurama.Attributed;

public sealed class RepairForCreation
{
    public string RepairDate { get; set; }
    public string Description { get; set; }
    public string Cost { get; set; }

}
