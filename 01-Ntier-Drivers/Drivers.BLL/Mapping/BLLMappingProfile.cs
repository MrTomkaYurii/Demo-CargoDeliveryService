using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using ADOEntities = Drivers.DAL.ADO.Entities;
using ADOHelp = Drivers.DAL.ADO.Entities.HelpModels;
using EFEntities = Drivers.DAL.EF.Entities;
using EFHelp = Drivers.DAL.EF.Entities.HelpModels;

namespace Drivers.BLL.Mapping;

/// <summary>
/// Профіль зіставлення AutoMapper для бізнес-логіки.
/// Забезпечує уніфікований мапінг як із сутностей Entity Framework Core,
/// так і з сутностей ADO.NET / Dapper в універсальні DTO відповіді.
/// </summary>
public class BLLMappingProfile : Profile
{
    public BLLMappingProfile()
    {
        // ----------------------------------------------------
        // ВОДІЇ (DRIVERS)
        // ----------------------------------------------------

        // Мапінг EFDriver -> DTO
        CreateMap<EFEntities.EFDriver, ShortDriverResponceDTO>();
        CreateMap<EFEntities.EFDriver, FullDriverResponceDTO>()
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses));

        // Мапінг ADO Driver -> DTO
        CreateMap<ADOEntities.Driver, ShortDriverResponceDTO>();
        CreateMap<ADOEntities.Driver, FullDriverResponceDTO>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyID))
            .ForMember(dest => dest.PhotoId, opt => opt.MapFrom(src => src.PhotoID))
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses));

        // Мапінг запитів на додавання/оновлення
        CreateMap<MiniDriverReqDTO, EFEntities.EFDriver>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore())
            .ForMember(dest => dest.Photo, opt => opt.Ignore())
            .ForMember(dest => dest.Expenses, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<MiniDriverReqDTO, ADOEntities.Driver>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore())
            .ForMember(dest => dest.Photo, opt => opt.Ignore())
            .ForMember(dest => dest.Expenses, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // ----------------------------------------------------
        // ВИТРАТИ (EXPENSES)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFExpense, ExpenseResponceDTO>()
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<ADOEntities.Expenses, ExpenseResponceDTO>()
            .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverID))
            .ForMember(dest => dest.TruckId, opt => opt.MapFrom(src => src.TruckID))
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<ExpenseReqDTO, EFEntities.EFExpense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<ExpenseReqDTO, ADOEntities.Expenses>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DriverID, opt => opt.MapFrom(src => src.DriverId))
            .ForMember(dest => dest.TruckID, opt => opt.MapFrom(src => src.TruckId))
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // ----------------------------------------------------
        // КОМПАНІЇ (COMPANIES)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFCompany, CompanyResponceDTO>();
        CreateMap<ADOEntities.Company, CompanyResponceDTO>();

        CreateMap<EFEntities.EFCompany, CompanyWithDriversResponseDTO>()
            .ForMember(dest => dest.Drivers, opt => opt.MapFrom(src => src.Driver != null ? new List<EFEntities.EFDriver> { src.Driver } : new List<EFEntities.EFDriver>()));
        CreateMap<ADOEntities.Company, CompanyWithDriversResponseDTO>()
            .ForMember(dest => dest.Drivers, opt => opt.MapFrom(src => src.Drivers));

        CreateMap<CompanyReqDTO, EFEntities.EFCompany>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<CompanyReqDTO, ADOEntities.Company>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Drivers, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // ----------------------------------------------------
        // ТЕХОГЛЯДИ (INSPECTIONS)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFInspection, InspectionResponceDTO>()
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<ADOEntities.Inspection, InspectionResponceDTO>()
            .ForMember(dest => dest.TruckId, opt => opt.MapFrom(src => src.TruckID))
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<InspectionReqDTO, EFEntities.EFInspection>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<InspectionReqDTO, ADOEntities.Inspection>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TruckID, opt => opt.MapFrom(src => src.TruckId))
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // ----------------------------------------------------
        // РЕМОНТИ (REPAIRS)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFRepair, RepairResponceDTO>()
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<ADOEntities.Repair, RepairResponceDTO>()
            .ForMember(dest => dest.TruckId, opt => opt.MapFrom(src => src.TruckID))
            .ForMember(dest => dest.Truck, opt => opt.MapFrom(src => src.Truck));

        CreateMap<RepairReqDTO, EFEntities.EFRepair>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<RepairReqDTO, ADOEntities.Repair>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TruckID, opt => opt.MapFrom(src => src.TruckId))
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        // ----------------------------------------------------
        // ФОТОГРАФІЇ (PHOTOS)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFPhoto, PhotoResponceDTO>();
        CreateMap<ADOEntities.Photo, PhotoResponceDTO>();

        CreateMap<PhotoReqDTO, EFEntities.EFPhoto>();
        CreateMap<PhotoReqDTO, ADOEntities.Photo>();

        // ----------------------------------------------------
        // ВАНТАЖІВКИ (TRUCKS)
        // ----------------------------------------------------
        CreateMap<EFEntities.EFTruck, TruckResponceDTO>();
        CreateMap<ADOEntities.Truck, TruckResponceDTO>();

        // ----------------------------------------------------
        // АНАЛІТИЧНІ ТА АГРЕГАТНІ ЗВІТИ (ANALYTICS)
        // ----------------------------------------------------
        CreateMap<EFHelp.DriverExpenseSummary, DriverExpenseSummaryResponseDTO>();
        CreateMap<ADOHelp.DriverExpenseSummary, DriverExpenseSummaryResponseDTO>();

        CreateMap<EFHelp.TopDriverExpense, TopDriverExpenseResponseDTO>();
        CreateMap<ADOHelp.TopDriverExpense, TopDriverExpenseResponseDTO>();

        CreateMap<EFHelp.DriverLicenseAlert, DriverLicenseAlertResponseDTO>();
        CreateMap<ADOHelp.DriverLicenseAlert, DriverLicenseAlertResponseDTO>();

        CreateMap<EFHelp.TruckMaintenanceSummary, TruckMaintenanceSummaryResponseDTO>();
        CreateMap<ADOHelp.TruckMaintenanceSummary, TruckMaintenanceSummaryResponseDTO>();
    }
}
