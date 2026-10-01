using System;

namespace SharedKernel.Messages
{
    public interface IDriverUpdated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DriverUpdated : IDriverUpdated
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}