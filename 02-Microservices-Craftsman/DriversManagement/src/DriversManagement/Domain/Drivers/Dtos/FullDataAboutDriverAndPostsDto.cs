using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

namespace DriversManagement.Domain.Drivers.Dtos
{
    public class FullDataAboutDriverAndPostsDto
    {
        public DriverDto Driver { get; set; } = new();
        public List<PostAboutDriverDto> Posts { get; set; } = new();

        public Guid Id => Driver?.Id ?? Guid.Empty;
        public string FirstName => Driver?.FirstName;
        public string LastName => Driver?.LastName;
        public string MiddleName => Driver?.MiddleName;
        public string Gender => Driver?.Gender;
        public string Birthdate => Driver?.Birthdate;
        public string PlaceOfBirth => Driver?.PlaceOfBirth;
        public string Nationality => Driver?.Nationality;
        public string MaritalStatus => Driver?.MaritalStatus;
        public string IdentificationType => Driver?.IdentificationType;
        public string IdentificationNumber => Driver?.IdentificationNumber;
        public string IdentificationExpirationDate => Driver?.IdentificationExpirationDate;
        public string Address => Driver?.Address;
        public string Phone => Driver?.Phone;
        public string Email => Driver?.Email;
        public string DriverLicenseNumber => Driver?.DriverLicenseNumber;
        public string DriverLicenseCategory => Driver?.DriverLicenseCategory;
        public string DriverLicenseIssuingDate => Driver?.DriverLicenseIssuingDate;
        public string DriverLicenseExpirationDate => Driver?.DriverLicenseExpirationDate;
        public string DriverLicenseIssuingAuthority => Driver?.DriverLicenseIssuingAuthority;
        public string EmploymentStatus => Driver?.EmploymentStatus;
        public string EmploymentStartDate => Driver?.EmploymentStartDate;
        public string EmploymentEndDate => Driver?.EmploymentEndDate;
    }
}
