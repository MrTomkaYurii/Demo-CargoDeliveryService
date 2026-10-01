namespace CargoOrderingService.FunctionalTests.TestUtilities;
public class ApiRoutes
{
    public const string Base = "api";
    public const string Health = Base + "/health";

    // new api route marker - do not delete

    public static class Deliveries
    {
        public static string GetList => $"{Base}/deliveries";
        public static string GetAll => $"{Base}/deliveries/all";
        public static string GetRecord(Guid id) => $"{Base}/deliveries/{id}";
        public static string Delete(Guid id) => $"{Base}/deliveries/{id}";
        public static string Put(Guid id) => $"{Base}/deliveries/{id}";
        public static string Create => $"{Base}/deliveries";
        public static string CreateBatch => $"{Base}/deliveries/batch";
    }

    public static class Orders
    {
        public static string GetList => $"{Base}/orders";
        public static string GetAll => $"{Base}/orders/all";
        public static string GetRecord(Guid id) => $"{Base}/orders/{id}";
        public static string Delete(Guid id) => $"{Base}/orders/{id}";
        public static string Put(Guid id) => $"{Base}/orders/{id}";
        public static string Create => $"{Base}/orders";
        public static string CreateBatch => $"{Base}/orders/batch";
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
