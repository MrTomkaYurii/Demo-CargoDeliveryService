using Drivers.DAL.ADO.Repositories.Contracts;
using Drivers.DAL.ADO.UOW.Contracts;
using System.Data;

namespace Drivers.DAL.ADO.UOW;

/// <summary>
/// Реалізація Unit of Work для ADO.NET / Dapper
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    public IDriverRepository DriverRepository { get; }
    public ICompanyRepository CompanyRepository { get; }
    public IExpensesRepository ExpensesRepository { get; }
    public IPhotoRepository PhotoRepository { get; }
    public IInspectionRepository InspectionRepository { get; }
    public IRepairRepository RepairRepository { get; }
    public ITruckRepository TruckRepository { get; }

    public IDriverRepository _driverRepository => DriverRepository;
    public ICompanyRepository _companyRepository => CompanyRepository;
    public IExpensesRepository _expensesRepository => ExpensesRepository;
    public IPhotoRepository _photoRepository => PhotoRepository;
    public IInspectionRepository _inspectionRepository => InspectionRepository;
    public IRepairRepository _repairRepository => RepairRepository;
    public ITruckRepository _truckRepository => TruckRepository;

    private readonly IDbTransaction _dbTransaction;

    public UnitOfWork(
        IDriverRepository driverRepository,
        ICompanyRepository companyRepository,
        IExpensesRepository expensesRepository,
        IInspectionRepository inspectionRepository,
        IPhotoRepository photoRepository,
        IRepairRepository repairRepository,
        ITruckRepository truckRepository,
        IDbTransaction dbTransaction)
    {
        DriverRepository = driverRepository;
        CompanyRepository = companyRepository;
        ExpensesRepository = expensesRepository;
        InspectionRepository = inspectionRepository;
        PhotoRepository = photoRepository;
        RepairRepository = repairRepository;
        TruckRepository = truckRepository;
        _dbTransaction = dbTransaction;
    }

    public void Commit()
    {
        try
        {
            _dbTransaction.Commit();
        }
        catch (Exception ex)
        {
            _dbTransaction.Rollback();
            Console.WriteLine(ex.Message);
        }
    }

    public void Dispose()
    {
        _dbTransaction.Connection?.Close();
        _dbTransaction.Connection?.Dispose();
        _dbTransaction.Dispose();
    }
}
