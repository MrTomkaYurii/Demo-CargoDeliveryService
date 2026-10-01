namespace Drivers.BLL.DTOs.Responses
{
    public class TruckResponceDTO
    {
        public int Id { get; set; }
        public string TruckNumber { get; set; } 
        public string Model { get; set; }
        public int Year { get; set; }
        public int Capacity { get; set; }
        public string? FuelType { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Vin { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? InsuranceExpirationDate { get; set; }
    }
}
