using System;

namespace SharedKernel.Messages
{
    public interface IDriverCreated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string Phone { get; set; }
    }

    public class DriverCreated : IDriverCreated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string Phone { get; set; }
    }
}
