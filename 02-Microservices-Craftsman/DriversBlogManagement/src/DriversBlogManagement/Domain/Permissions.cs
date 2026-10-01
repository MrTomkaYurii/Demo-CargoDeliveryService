namespace DriversBlogManagement.Domain;

using System.Reflection;

public static class Permissions
{
    // Permissions marker - do not delete this comment
    public const string CanDeleteUser = nameof(CanDeleteUser);
    public const string CanUpdateUser = nameof(CanUpdateUser);
    public const string CanAddUser = nameof(CanAddUser);
    public const string CanReadUser = nameof(CanReadUser);
    public const string CanDeleteComment = nameof(CanDeleteComment);
    public const string CanUpdateComment = nameof(CanUpdateComment);
    public const string CanAddComment = nameof(CanAddComment);
    public const string CanReadComment = nameof(CanReadComment);
    public const string CanReadComments = nameof(CanReadComments);
    public const string CanDeletePostAboutDriver = nameof(CanDeletePostAboutDriver);
    public const string CanUpdatePostAboutDriver = nameof(CanUpdatePostAboutDriver);
    public const string CanAddPostAboutDriver = nameof(CanAddPostAboutDriver);
    public const string CanReadPostAboutDriver = nameof(CanReadPostAboutDriver);
    public const string CanReadPostAboutDrivers = nameof(CanReadPostAboutDrivers);
    public const string CanDeleteDriver = nameof(CanDeleteDriver);
    public const string CanUpdateDriver = nameof(CanUpdateDriver);
    public const string CanAddDriver = nameof(CanAddDriver);
    public const string CanReadDriver = nameof(CanReadDriver);
    public const string CanReadDrivers = nameof(CanReadDrivers);
    public const string CanDeleteLike = nameof(CanDeleteLike);
    public const string CanUpdateLike = nameof(CanUpdateLike);
    public const string CanAddLike = nameof(CanAddLike);
    public const string CanReadLike = nameof(CanReadLike);
    public const string CanReadLikes = nameof(CanReadLikes);
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
