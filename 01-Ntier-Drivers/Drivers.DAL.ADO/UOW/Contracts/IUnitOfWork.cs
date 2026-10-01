using Drivers.DAL.ADO.Repositories.Contracts;

namespace Drivers.DAL.ADO.UOW.Contracts;

/// <summary>
/// Інтерфейс Unit of Work для ADO.NET / Dapper
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDriverRepository DriverRepository { get; }
    ICompanyRepository CompanyRepository { get; }
    IExpensesRepository ExpensesRepository { get; }
    IInspectionRepository InspectionRepository { get; }
    IPhotoRepository PhotoRepository { get; }
    IRepairRepository RepairRepository { get; }
    ITruckRepository TruckRepository { get; }

    // Зворотна сумісність для старих звернень
    IDriverRepository _driverRepository => DriverRepository;
    ICompanyRepository _companyRepository => CompanyRepository;
    IExpensesRepository _expensesRepository => ExpensesRepository;
    IInspectionRepository _inspectionRepository => InspectionRepository;
    IPhotoRepository _photoRepository => PhotoRepository;
    IRepairRepository _repairRepository => RepairRepository;
    ITruckRepository _truckRepository => TruckRepository;

    void Commit();
}
