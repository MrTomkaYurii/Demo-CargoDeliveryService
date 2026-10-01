namespace CargoOrderingService.Domain.Orders.Dtos;

using Destructurama.Attributed;

public sealed record OrderForCreationDto
{
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public string DeliveryDate { get; set; }
    public string TotalAmount { get; set; }
    public string Status { get; set; }

}
