using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.UOW.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Drivers.DAL.EF.UOW;

public class EFUnitOfWork : IEFUnitOfWork
{
    private readonly DriversManagementContext databaseContext;
    private IDbContextTransaction transaction;

    public IEFDriverRepository EFDriverRepository { get; }

    public IEFCompanyRepository EFCompanyRepository { get; }

    public IEFExpenseRepository EFExpenseRepository { get; }

    public IEFInspectionRepository EFInspectionRepository { get; }

    public IEFPhotoRepository EFPhotoRepository { get; }

    public IEFRepairRepository EFPRepairRepository { get; }

    public IEFTruckRepository EFPTruckRepository { get; }

    public EFUnitOfWork(
        DriversManagementContext databaseContext,
        IEFDriverRepository eFDriverRepository,
        IEFCompanyRepository eFCompanyRepository,
        IEFExpenseRepository eFExpenseRepository,
        IEFInspectionRepository eFInspectionRepository,
        IEFPhotoRepository eFPhotoRepository,
        IEFRepairRepository eFRepairRepository,
        IEFTruckRepository eFTruckRepository)
    {
        this.databaseContext = databaseContext;
        EFDriverRepository = eFDriverRepository;
        EFCompanyRepository = eFCompanyRepository;
        EFExpenseRepository = eFExpenseRepository;
        EFPhotoRepository = eFPhotoRepository;
        EFInspectionRepository = eFInspectionRepository;
        EFPRepairRepository = eFRepairRepository;
        EFPTruckRepository = eFTruckRepository;
    }

    /*    public async Task SaveChangesAsync()
        {
            await databaseContext.SaveChangesAsync();
        }*/

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync(cancellationToken);
    }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        transaction?.Dispose();

        databaseContext.Dispose();
    }
}