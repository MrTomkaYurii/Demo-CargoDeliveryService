namespace CargoOrderingService.Domain.Orders.Models;

using Destructurama.Attributed;

public sealed record OrderForUpdate
{
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public string DeliveryDate { get; set; }
    public string TotalAmount { get; set; }
    public string Status { get; set; }

}
