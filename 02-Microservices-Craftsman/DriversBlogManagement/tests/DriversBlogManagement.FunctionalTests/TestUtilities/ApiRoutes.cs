namespace DriversBlogManagement.FunctionalTests.TestUtilities;
public class ApiRoutes
{
    public const string Base = "api";
    public const string Health = Base + "/health";

    // new api route marker - do not delete

    public static class BlogUsers
    {
        public static string GetList => $"{Base}/blogUsers";
        public static string GetAll => $"{Base}/blogUsers/all";
        public static string GetRecord(Guid id) => $"{Base}/blogUsers/{id}";
        public static string Delete(Guid id) => $"{Base}/blogUsers/{id}";
        public static string Put(Guid id) => $"{Base}/blogUsers/{id}";
        public static string Create => $"{Base}/blogUsers";
        public static string CreateBatch => $"{Base}/blogUsers/batch";
    }

    public static class Comments
    {
        public static string GetList => $"{Base}/comments";
        public static string GetAll => $"{Base}/comments/all";
        public static string GetRecord(Guid id) => $"{Base}/comments/{id}";
        public static string Delete(Guid id) => $"{Base}/comments/{id}";
        public static string Put(Guid id) => $"{Base}/comments/{id}";
        public static string Create => $"{Base}/comments";
        public static string CreateBatch => $"{Base}/comments/batch";
    }

    public static class PostAboutDrivers
    {
        public static string GetList => $"{Base}/postAboutDrivers";
        public static string GetAll => $"{Base}/postAboutDrivers/all";
        public static string GetRecord(Guid id) => $"{Base}/postAboutDrivers/{id}";
        public static string Delete(Guid id) => $"{Base}/postAboutDrivers/{id}";
        public static string Put(Guid id) => $"{Base}/postAboutDrivers/{id}";
        public static string Create => $"{Base}/postAboutDrivers";
        public static string CreateBatch => $"{Base}/postAboutDrivers/batch";
    }

    public static class Drivers
    {
        public static string GetList => $"{Base}/drivers";
        public static string GetAll => $"{Base}/drivers/all";
        public static string GetRecord(Guid id) => $"{Base}/drivers/{id}";
        public static string Delete(Guid id) => $"{Base}/drivers/{id}";
        public static string Put(Guid id) => $"{Base}/drivers/{id}";
        public static string Create => $"{Base}/drivers";
        public static string CreateBatch => $"{Base}/drivers/batch";
    }

    public static class Likes
    {
        public static string GetList => $"{Base}/likes";
        public static string GetAll => $"{Base}/likes/all";
        public static string GetRecord(Guid id) => $"{Base}/likes/{id}";
        public static string Delete(Guid id) => $"{Base}/likes/{id}";
        public static string Put(Guid id) => $"{Base}/likes/{id}";
        public static string Create => $"{Base}/likes";
        public static string CreateBatch => $"{Base}/likes/batch";
    }

    public static class Users
    {
        public static string GetList => $"{Base}/users";
        public static string GetRecord(Guid id) => $"{Base}/users/{id}";
        public static string Delete(Guid id) => $"{Base}/users/{id}";
        public static string Put(Guid id) => $"{Base}/users/{id}";
        public static string Create => $"{Base}/users";
        public static string CreateBatch => $"{Base}/users/batch";
        public static string AddRole(Guid id) => $"{Base}/users/{id}/addRole";
        public static string RemoveRole(Guid id) => $"{Base}/users/{id}/removeRole";
    }

    public static class RolePermissions
    {
        public static string GetList => $"{Base}/rolePermissions";
        public static string GetAll => $"{Base}/rolePermissions/all";
        public static string GetRecord(Guid id) => $"{Base}/rolePermissions/{id}";
        public static string Delete(Guid id) => $"{Base}/rolePermissions/{id}";
        public static string Put(Guid id) => $"{Base}/rolePermissions/{id}";
        public static string Create => $"{Base}/rolePermissions";
        public static string CreateBatch => $"{Base}/rolePermissions/batch";
    }
}
