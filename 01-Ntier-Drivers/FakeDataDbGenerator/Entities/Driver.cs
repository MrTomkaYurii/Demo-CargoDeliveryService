using System;
using System.Collections.Generic;

namespace FakeDataDriverDbGenerator.Entities
{
    public partial class Driver
    {
        public Driver() => Expenses = new HashSet<Expense>();

        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatus { get; set; }
        public string? IdentificationType { get; set; }
        public string? IdentificationNumber { get; set; }
        public DateTime? IdentificationExpirationDate { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string? DriverLicenseCategory { get; set; }
        public DateTime DriverLicenseIssuingDate { get; set; }
        public DateTime DriverLicenseExpirationDate { get; set; }
        public string? DriverLicenseIssuingAuthority { get; set; }
        public string? EmploymentStatus { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int? CompanyId { get; set; }
        public int? PhotoId { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Photo? Photo { get; set; }

        public virtual ICollection<Expense> Expenses { get; set; }
    }
}
