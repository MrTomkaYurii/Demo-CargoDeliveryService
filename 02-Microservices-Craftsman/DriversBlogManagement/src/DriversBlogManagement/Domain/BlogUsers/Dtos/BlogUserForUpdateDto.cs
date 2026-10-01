namespace DriversBlogManagement.Domain.BlogUsers.Dtos;

using Destructurama.Attributed;

public sealed record BlogUserForUpdateDto
{
    public string UserName { get; set; }
    public string Email { get; set; }

}
