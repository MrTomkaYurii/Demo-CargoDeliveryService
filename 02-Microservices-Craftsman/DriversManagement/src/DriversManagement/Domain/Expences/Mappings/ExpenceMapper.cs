namespace DriversManagement.Domain.Expences.Mappings;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class ExpenceMapper
{
    public static partial ExpenceForCreation ToExpenceForCreation(this ExpenceForCreationDto expenceForCreationDto);
    public static partial ExpenceForUpdate ToExpenceForUpdate(this ExpenceForUpdateDto expenceForUpdateDto);
    public static partial ExpenceDto ToExpenceDto(this Expence expence);
    public static partial IQueryable<ExpenceDto> ToExpenceDtoQueryable(this IQueryable<Expence> expence);
}