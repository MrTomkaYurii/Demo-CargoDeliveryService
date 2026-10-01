using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.DAL.EF.Entities;

namespace Drivers.Api.AutomapperProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            /////////////////////////////////////////////////////////////////////
            ///   Responces
            /////////////////////////////////////////////////////////////////////
            CreateMap<EFDriver, FullDriverResponceDTO>().ReverseMap();
            CreateMap<EFDriver, ShortDriverResponceDTO>().ReverseMap();
            CreateMap<EFCompany, CompanyResponceDTO>().ReverseMap ();

            /////////////////////////////////////////////////////////////////////
            ///   Requests
            /////////////////////////////////////////////////////////////////////
            CreateMap<EFDriver, MiniDriverReqDTO>().ReverseMap();
            CreateMap<EFPhoto, PhotoReqDTO>().ReverseMap();
        }
    }
}
