namespace DriversManagement.Domain.Companies.Dtos;

using Destructurama.Attributed;

public sealed record CompanyForUpdateDto
{
    public string CompanyName { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPerson { get; set; }
    public string ContactPhone { get; set; }
}
