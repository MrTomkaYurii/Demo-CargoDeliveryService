namespace CargoOrderingService.Domain.Deliveries.Services;

using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Databases;
using CargoOrderingService.Services;

public interface IDeliveryRepository : IGenericRepository<Delivery>
{
}

public sealed class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
{
    private readonly OrderingContext _dbContext;

    public DeliveryRepository(OrderingContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
