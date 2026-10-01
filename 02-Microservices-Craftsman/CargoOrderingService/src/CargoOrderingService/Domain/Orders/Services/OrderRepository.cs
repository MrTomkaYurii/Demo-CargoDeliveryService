namespace CargoOrderingService.Domain.Orders.Services;

using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Databases;
using CargoOrderingService.Services;

public interface IOrderRepository : IGenericRepository<Order>
{
}

public sealed class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly OrderingContext _dbContext;

    public OrderRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
