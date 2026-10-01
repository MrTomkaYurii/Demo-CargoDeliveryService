using Bogus;
using FakeDataDriverDbGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FakeDataDriverDbGenerator.Seeders
{
    public enum Gender
    {
        Male,
        Female
    }
    public enum ExpenseCategory
    {
        Fuel,
        Fine,
        Maintenance
    }

    public class DriversManagementDatabaseSeeder
    {
        private int quantity;
        public IReadOnlyCollection<Company> Companies { get; } = new List<Company>();
        public IReadOnlyCollection<Driver> Drivers { get; } = new List<Driver>();
        public IReadOnlyCollection<Expense> Expenses { get; } = new List<Expense>();
        public IReadOnlyCollection<Inspection> Inspections { get; } = new List<Inspection>();
        public IReadOnlyCollection<Repair> Repairs { get; } = new List<Repair>();
        public IReadOnlyCollection<Truck> Trucks { get; } = new List<Truck>();
        public IReadOnlyCollection<Photo> Photos { get; } = new List<Photo>();

        public DriversManagementDatabaseSeeder(int number_of_rows = 50)
        {
            quantity = number_of_rows;
            
            Trucks = GenerateTrucks(amount: quantity);
            Inspections = GenerateInspections(amount: quantity, Trucks);
            Repairs = GenerateRepairs(amount: quantity, Trucks);

            Companies = GenerateCompanies(amount: quantity);
            Photos = GeneratePhotos(amount: quantity);
            Drivers = GenerateDrivers(amount: quantity, Companies, Photos);

            Expenses = GenerateExpenses(amount: quantity, Drivers, Trucks);
        }

        /// <summary>
        /// Генерація компаній
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Company> GenerateCompanies(int amount)
        {
            var Id = 1;
            var companyFaker = new Faker<Company>()
                    .RuleFor(c => c.Id, f => Id++)
                    .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
                    .RuleFor(c => c.Address, f => f.Address.FullAddress())
                    .RuleFor(c => c.Phone, f => f.Phone.PhoneNumberFormat())
                    .RuleFor(c => c.Email, f => f.Internet.Email())
                    .RuleFor(c => c.ContactPerson, f => f.Name.FullName())
                    .RuleFor(c => c.ContactPhone, f => f.Phone.PhoneNumberFormat())
                    .RuleFor(c => c.ContactEmail, f => f.Internet.Email())
                    .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                    .RuleFor(c => c.UpdatedAt, f => null as DateTime?)
                    .RuleFor(c => c.DeletedAt, f => null as DateTime?)
                    .RuleFor(c => c.Driver, f => null);
            var companies = Enumerable.Range(1, amount)
                    .Select(i => SeedRow(companyFaker, i))
                    .ToList();
            return companies;
        }

        /// <summary>
        /// Генерація фотографій
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Photo> GeneratePhotos(int amount)
        {
            var Id = 1;
            var photoFaker = new Faker<Photo>()
                .RuleFor(c => c.Id, f => Id++)
                //.RuleFor(c => c.PhotoData, f => Convert.FromBase64String(f.Image.DataUri(100, 100).Split(',')[1]))
                //надо сгенерировать картинку в строке и потом кинуть в масив байтов
                .RuleFor(c => c.PhotoData, f => f.Random.Bytes(1024))
                .RuleFor(c => c.ContentType, (f, u) => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.FileName, f => f.Lorem.Word())
                .RuleFor(c => c.FileSize, f => f.Random.Number(1000, 50000))
                .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                .RuleFor(c => c.UpdatedAt, f => null as DateTime?)
                .RuleFor(c => c.DeletedAt, f => null as DateTime?)
                .RuleFor(c => c.Driver, f => null);
            var photos = Enumerable.Range(1, amount)
                .Select(i => SeedRow(photoFaker, i))
                .ToList();
            return photos;
        }

        /// <summary>
        /// Генерація водіїв
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="companies"></param>
        /// <param name="photos"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Driver> GenerateDrivers(int amount,
            IEnumerable<Company> companies,
            IEnumerable<Photo> photos)
        {
            var Id = 1;
            var CounterCompany = 0;
            var CounterPhoto = 0;
            var driverFaker = new Faker<Driver>()
                .RuleFor(d => d.Id, f => Id++)
                .RuleFor(d => d.FirstName, f => f.Name.FirstName())
                .RuleFor(d => d.LastName, f => f.Name.LastName())
                .RuleFor(d => d.MiddleName, f => f.Name.LastName())
                .RuleFor(d => d.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
                .RuleFor(d => d.Birthdate, f => f.Date.Past(30))
                .RuleFor(d => d.PlaceOfBirth, f => f.Address.City())
                .RuleFor(d => d.Nationality, f => f.Address.Country())
                .RuleFor(d => d.MaritalStatus, f => f.PickRandom(new[] { "Single", "Married", "Divorced", "Widowed" }))
                .RuleFor(d => d.IdentificationType, f => f.PickRandom(new[] { "Passport", "National ID", "Driver's License" }))
                .RuleFor(d => d.IdentificationNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(d => d.IdentificationExpirationDate, f => f.Date.Future())
                .RuleFor(d => d.Address, f => f.Address.FullAddress())
                .RuleFor(d => d.Phone, f => f.Phone.PhoneNumberFormat())
                .RuleFor(d => d.Email, f => f.Person.Email)
                .RuleFor(d => d.DriverLicenseNumber, f => f.Random.AlphaNumeric(10))
                .RuleFor(d => d.DriverLicenseCategory, f => f.PickRandom(new[] { "A", "B", "C", "D", "E" }))
                .RuleFor(d => d.DriverLicenseIssuingDate, f => f.Date.Past(10))
                .RuleFor(d => d.DriverLicenseExpirationDate, f => f.Date.Future())
                .RuleFor(d => d.DriverLicenseIssuingAuthority, f => f.Address.City())
                .RuleFor(d => d.EmploymentStatus, f => f.PickRandom(new[] { "Full-Time", "Part-Time", "Contractor", "Freelance" }))
                .RuleFor(d => d.EmploymentStartDate, f => f.Date.Past(1))
                .RuleFor(d => d.EmploymentEndDate, f => f.Date.Future(5))
                //.RuleFor(d => d.CompanyId, f => f.PickRandom(companies).Id)
                //.RuleFor(d => d.PhotoId, f => f. PickRandom(photos).Id)
                .RuleFor(d => d.CompanyId, f =>
                {
                    var companyId = companies.ElementAt(CounterCompany).Id;
                    CounterCompany++;
                    return companyId;
                })
                .RuleFor(d => d.PhotoId, f =>
                {
                    var photoId = photos.ElementAt(CounterPhoto).Id;
                    CounterPhoto++;
                    return photoId;
                })
                .RuleFor(d => d.CreatedAt, f => f.Date.Past(1))
                .RuleFor(d => d.UpdatedAt, f => null as DateTime?)
                .RuleFor(d => d.DeletedAt, f => null as DateTime?);

            var drivers = Enumerable.Range(1, amount)
                .Select(i => SeedRow(driverFaker, i))
                .ToList();
            return drivers;
        }

        /// <summary>
        /// Генерація машин
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Truck> GenerateTrucks(int amount)
        {
            var Id = 1;
            var truckFaker = new Faker<Truck>()
                 .RuleFor(t => t.Id, f => Id++)
                 .RuleFor(c => c.TruckNumber, f => f.Vehicle.Vin())
                 .RuleFor(c => c.Model, f => f.Vehicle.Model())
                 .RuleFor(c => c.Year, f => f.Date.Past().Year)
                 .RuleFor(c => c.Capacity, f => f.Random.Number(100, 1000))
                 .RuleFor(c => c.FuelType, f => f.Vehicle.Fuel())
                 .RuleFor(c => c.FuelConsumption, f => f.Random.Decimal(1, 5))
                 .RuleFor(c => c.RegistrationNumber, f => f.Vehicle.Vin())
                 .RuleFor(c => c.Vin, f => f.Vehicle.Vin())
                 .RuleFor(c => c.EngineNumber, f => Guid.NewGuid().ToString())
                 .RuleFor(c => c.ChassisNumber, f => Guid.NewGuid().ToString())
                 .RuleFor(c => c.InsurancePolicyNumber, f => Guid.NewGuid().ToString())
                 .RuleFor(c => c.InsuranceExpirationDate, f => f.Date.Future())
                 .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                 .RuleFor(c => c.UpdatedAt, f => null as DateTime?)
                 .RuleFor(c => c.DeletedAt, f => null as DateTime?);
            var trucks = Enumerable.Range(1, amount)
                .Select(i => SeedRow(truckFaker, i))
                .ToList();
            return trucks;
        }

        /// <summary>
        /// Генерація інспекцій
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="trucks"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Inspection> GenerateInspections(int amount, IEnumerable<Truck> trucks)
        {
            var Id = 1;
            var inspectionFaker = new Faker<Inspection>()
                .RuleFor(c => c.Id, f => Id++)
                .RuleFor(c => c.InspectionDate, f => f.Date.Past())
                .RuleFor(c => c.Description, f => f.Lorem.Text())
                .RuleFor(c => c.Result, f => f.Random.Bool())
                .RuleFor(c => c.TruckId, f => f.PickRandom(trucks).Id)
                .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                .RuleFor(c => c.UpdatedAt, f => null as DateTime?)
                .RuleFor(c => c.DeletedAt, f => null as DateTime?)
                .RuleFor(c => c.Truck, f => null);
            var inspections = Enumerable.Range(1, amount)
                .Select(i => SeedRow(inspectionFaker, i))
                .ToList();
            return inspections;
        }

        /// <summary>
        /// Генерація ремонтів
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="trucks"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Repair> GenerateRepairs(int amount, IEnumerable<Truck> trucks)
        {
            var Id = 1;
            var RepairFaker = new Faker<Repair>()
                .RuleFor(c => c.Id, f => Id++)
                .RuleFor(c => c.RepairDate, f => f.Date.Past())
                .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                .RuleFor(c => c.Cost, f => f.Finance.Amount())
                .RuleFor(c => c.TruckId, f => f.PickRandom(trucks).Id)
                .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                .RuleFor(c => c.UpdatedAt, f => null as DateTime?)
                .RuleFor(c => c.DeletedAt, f => null as DateTime?)
                .RuleFor(c => c.Truck, f => null);
            var repairs = Enumerable.Range(1, amount)
                .Select(i => SeedRow(RepairFaker, i))
                .ToList();
            return repairs;
        }

        /// <summary>
        /// Генерація вартості. Виступає також як проміжна таблиця
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private IReadOnlyCollection<Expense> GenerateExpenses(int amount,
            IEnumerable<Driver> drivers,
            IEnumerable<Truck> trucks)
        {
            var Id = 1;
            var ExpenseFaker = new Faker<Expense>()
                .RuleFor(e => e.Id, f => Id++)
                .RuleFor(e => e.DriverId, f => f.PickRandom(drivers).Id)
                .RuleFor(e => e.TruckId, f => f.PickRandom(trucks).Id)
                .RuleFor(e => e.DriverPayment, f => f.Finance.Amount())
                .RuleFor(e => e.FuelCost, f => f.Finance.Amount())
                .RuleFor(e => e.MaintenanceCost, f => f.Finance.Amount())
                .RuleFor(e => e.Category, f => f.Commerce.Categories(1)[0])
                .RuleFor(e => e.Date, f => f.Date.Past())
                .RuleFor(e => e.Note, f => f.Rant.Review())
                .RuleFor(e => e.CreatedAt, f => f.Date.Past())
                .RuleFor(e => e.UpdatedAt, f => null as DateTime?)
                .RuleFor(e => e.DeletedAt, f => null as DateTime?)
                .RuleFor(e => e.Driver, f => null as Driver)
                .RuleFor(e => e.Truck, f => null as Truck);

            var expenses = Enumerable.Range(1, amount)
                .Select(i => SeedRow(ExpenseFaker, i))
                // We do this GroupBy() + Select() to remove the duplicates from the generated join table entities
                .GroupBy(e => new { e.DriverId, e.TruckId })
                .Select(e => e.First())
                .ToList();

            return expenses;
        }

        /// <summary>
        /// Допомійжний метод
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="faker"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        private static T SeedRow<T>(Faker<T> faker, int rowId) where T : class
        {
            var recordRow = faker.UseSeed(rowId).Generate();
            return recordRow;
        }
    }
}
