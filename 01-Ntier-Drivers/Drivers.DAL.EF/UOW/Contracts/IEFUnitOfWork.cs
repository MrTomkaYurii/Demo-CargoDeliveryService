using Drivers.DAL.EF.Repositories.Contracts;

namespace Drivers.DAL.EF.UOW.Contracts;

public interface IEFUnitOfWork
{
    IEFDriverRepository EFDriverRepository { get; }
    IEFCompanyRepository EFCompanyRepository { get; }
    IEFExpenseRepository EFExpenseRepository { get; }
    IEFInspectionRepository EFInspectionRepository { get; }
    IEFPhotoRepository EFPhotoRepository { get; }
    IEFRepairRepository EFPRepairRepository { get; }
    IEFTruckRepository EFPTruckRepository { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
    Task<int> CompleteAsync(CancellationToken cancellationToken);
    void Dispose();
}
