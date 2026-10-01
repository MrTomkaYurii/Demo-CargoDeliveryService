namespace DriversManagement.Domain.Expences.Dtos;

using Destructurama.Attributed;

public sealed record ExpenceDto
{
    public Guid Id { get; set; }
    public string DriverPaiment { get; set; }
    public string FuelCost { get; set; }
    public string MaintanceCost { get; set; }
    public string Category { get; set; }
    public string Date { get; set; }
    public string Note { get; set; }

}
