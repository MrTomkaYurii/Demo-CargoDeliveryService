namespace DriversManagement.Domain.Inspections.Mappings;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class InspectionMapper
{
    public static partial InspectionForCreation ToInspectionForCreation(this InspectionForCreationDto inspectionForCreationDto);
    public static partial InspectionForUpdate ToInspectionForUpdate(this InspectionForUpdateDto inspectionForUpdateDto);
    public static partial InspectionDto ToInspectionDto(this Inspection inspection);
    public static partial IQueryable<InspectionDto> ToInspectionDtoQueryable(this IQueryable<Inspection> inspection);
}