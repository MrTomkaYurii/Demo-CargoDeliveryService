namespace CargoOrderingService.Domain.Orders.Dtos;

using Destructurama.Attributed;

public sealed record OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public string DeliveryDate { get; set; }
    public string TotalAmount { get; set; }
    public string Status { get; set; }

}
