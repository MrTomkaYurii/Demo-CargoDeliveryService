namespace DriversManagement.Domain.Expences.Models;

using Destructurama.Attributed;

public sealed class ExpenceForUpdate
{
    public string DriverPaiment { get; set; }
    public string FuelCost { get; set; }
    public string MaintanceCost { get; set; }
    public string Category { get; set; }
    public string Date { get; set; }
    public string Note { get; set; }

}
