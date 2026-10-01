namespace Drivers.BLL.DTOs.Responses
{
    public class CompanyResponceDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }
}
