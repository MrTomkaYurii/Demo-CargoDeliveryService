using Drivers.BLL.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.BLL.DTOs.Requests
{
    public class MiniDriverReqDTO
    {
        public int id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public string? DriverLicenseNumber { get; set; }
        public DateTime? DriverLicenseExpirationDate { get; set; }
    }
}
