using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.BLL.DTOs.Responses
{
    public class ShortDriverResponceDTO
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public decimal? DriverPayment { get; set; }
        public string? TruckModel { get; set; }
    }
}
