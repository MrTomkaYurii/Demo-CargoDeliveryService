namespace DriversManagement.Domain.Inspections.Dtos;

using Destructurama.Attributed;

public sealed record InspectionForCreationDto
{
    public string InspectionDate { get; set; }
    public string Description { get; set; }
    public string Result { get; set; }

}
