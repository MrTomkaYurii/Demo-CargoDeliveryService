namespace DriversManagement.Domain;

using System.Reflection;

public static class Permissions
{
    // Permissions marker - do not delete this comment
    public const string CanDeleteExpence = nameof(CanDeleteExpence);
    public const string CanUpdateExpence = nameof(CanUpdateExpence);
    public const string CanAddExpence = nameof(CanAddExpence);
    public const string CanReadExpence = nameof(CanReadExpence);
    public const string CanReadExpences = nameof(CanReadExpences);
    public const string CanDeleteTruck = nameof(CanDeleteTruck);
    public const string CanUpdateTruck = nameof(CanUpdateTruck);
    public const string CanAddTruck = nameof(CanAddTruck);
    public const string CanReadTruck = nameof(CanReadTruck);
    public const string CanReadTrucks = nameof(CanReadTrucks);
    public const string CanDeleteInspection = nameof(CanDeleteInspection);
    public const string CanUpdateInspection = nameof(CanUpdateInspection);
    public const string CanAddInspection = nameof(CanAddInspection);
    public const string CanReadInspection = nameof(CanReadInspection);
    public const string CanReadInspections = nameof(CanReadInspections);
    public const string CanDeleteRepair = nameof(CanDeleteRepair);
    public const string CanUpdateRepair = nameof(CanUpdateRepair);
    public const string CanAddRepair = nameof(CanAddRepair);
    public const string CanReadRepair = nameof(CanReadRepair);
    public const string CanReadRepairs = nameof(CanReadRepairs);
    public const string CanDeletePhoto = nameof(CanDeletePhoto);
    public const string CanUpdatePhoto = nameof(CanUpdatePhoto);
    public const string CanAddPhoto = nameof(CanAddPhoto);
    public const string CanReadPhoto = nameof(CanReadPhoto);
    public const string CanReadPhotos = nameof(CanReadPhotos);
    public const string CanDeleteCompany = nameof(CanDeleteCompany);
    public const string CanUpdateCompany = nameof(CanUpdateCompany);
    public const string CanAddCompany = nameof(CanAddCompany);
    public const string CanReadCompany = nameof(CanReadCompany);
    public const string CanReadCompanies = nameof(CanReadCompanies);
    public const string CanDeleteDrivers = nameof(CanDeleteDrivers);
    public const string CanUpdateDriver = nameof(CanUpdateDriver);
    public const string CanAddDriver = nameof(CanAddDriver);
    public const string CanReadDriver = nameof(CanReadDriver);
    public const string CanReadDrivers = nameof(CanReadDrivers);
    public const string CanDeleteUsers = nameof(CanDeleteUsers);
    public const string CanUpdateUsers = nameof(CanUpdateUsers);
    public const string CanAddUsers = nameof(CanAddUsers);
    public const string CanReadUsers = nameof(CanReadUsers);
    public const string CanDeleteRolePermissions = nameof(CanDeleteRolePermissions);
    public const string CanUpdateRolePermissions = nameof(CanUpdateRolePermissions);
    public const string CanAddRolePermissions = nameof(CanAddRolePermissions);
    public const string CanReadRolePermissions = nameof(CanReadRolePermissions);
    public const string HangfireAccess = nameof(HangfireAccess);
    public const string CanRemoveUserRoles = nameof(CanRemoveUserRoles);
    public const string CanAddUserRoles = nameof(CanAddUserRoles);
    public const string CanGetRoles = nameof(CanGetRoles);
    public const string CanGetPermissions = nameof(CanGetPermissions);
    
    public static List<string> List()
    {
        return typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue())
            .ToList();
    }
}
