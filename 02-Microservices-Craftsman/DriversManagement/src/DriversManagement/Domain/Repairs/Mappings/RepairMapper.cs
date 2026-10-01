namespace DriversManagement.Domain.Repairs.Mappings;

using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class RepairMapper
{
    public static partial RepairForCreation ToRepairForCreation(this RepairForCreationDto repairForCreationDto);
    public static partial RepairForUpdate ToRepairForUpdate(this RepairForUpdateDto repairForUpdateDto);
    public static partial RepairDto ToRepairDto(this Repair repair);
    public static partial IQueryable<RepairDto> ToRepairDtoQueryable(this IQueryable<Repair> repair);
}