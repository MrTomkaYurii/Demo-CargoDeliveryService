namespace DriversManagement.FunctionalTests.TestUtilities;
public class ApiRoutes
{
    public const string Base = "api";
    public const string Health = Base + "/health";

    // new api route marker - do not delete

    public static class Expences
    {
        public static string GetList => $"{Base}/expences";
        public static string GetAll => $"{Base}/expences/all";
        public static string GetRecord(Guid id) => $"{Base}/expences/{id}";
        public static string Delete(Guid id) => $"{Base}/expences/{id}";
        public static string Put(Guid id) => $"{Base}/expences/{id}";
        public static string Create => $"{Base}/expences";
        public static string CreateBatch => $"{Base}/expences/batch";
    }

    public static class Trucks
    {
        public static string GetList => $"{Base}/trucks";
        public static string GetAll => $"{Base}/trucks/all";
        public static string GetRecord(Guid id) => $"{Base}/trucks/{id}";
        public static string Delete(Guid id) => $"{Base}/trucks/{id}";
        public static string Put(Guid id) => $"{Base}/trucks/{id}";
        public static string Create => $"{Base}/trucks";
        public static string CreateBatch => $"{Base}/trucks/batch";
    }

    public static class Inspections
    {
        public static string GetList => $"{Base}/inspections";
        public static string GetAll => $"{Base}/inspections/all";
        public static string GetRecord(Guid id) => $"{Base}/inspections/{id}";
        public static string Delete(Guid id) => $"{Base}/inspections/{id}";
        public static string Put(Guid id) => $"{Base}/inspections/{id}";
        public static string Create => $"{Base}/inspections";
        public static string CreateBatch => $"{Base}/inspections/batch";
    }

    public static class Repairs
    {
        public static string GetList => $"{Base}/repairs";
        public static string GetAll => $"{Base}/repairs/all";
        public static string GetRecord(Guid id) => $"{Base}/repairs/{id}";
        public static string Delete(Guid id) => $"{Base}/repairs/{id}";
        public static string Put(Guid id) => $"{Base}/repairs/{id}";
        public static string Create => $"{Base}/repairs";
        public static string CreateBatch => $"{Base}/repairs/batch";
    }

    public static class Photos
    {
        public static string GetList => $"{Base}/photos";
        public static string GetAll => $"{Base}/photos/all";
        public static string GetRecord(Guid id) => $"{Base}/photos/{id}";
        public static string Delete(Guid id) => $"{Base}/photos/{id}";
        public static string Put(Guid id) => $"{Base}/photos/{id}";
        public static string Create => $"{Base}/photos";
        public static string CreateBatch => $"{Base}/photos/batch";
    }

    public static class Companies
    {
        public static string GetList => $"{Base}/companies";
        public static string GetAll => $"{Base}/companies/all";
        public static string GetRecord(Guid id) => $"{Base}/companies/{id}";
        public static string Delete(Guid id) => $"{Base}/companies/{id}";
        public static string Put(Guid id) => $"{Base}/companies/{id}";
        public static string Create => $"{Base}/companies";
        public static string CreateBatch => $"{Base}/companies/batch";
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
