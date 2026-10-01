namespace DriversManagement.Domain.Inspections.Models;

using Destructurama.Attributed;

public sealed class InspectionForUpdate
{
    public string InspectionDate { get; set; }
    public string Description { get; set; }
    public string Result { get; set; }

}
