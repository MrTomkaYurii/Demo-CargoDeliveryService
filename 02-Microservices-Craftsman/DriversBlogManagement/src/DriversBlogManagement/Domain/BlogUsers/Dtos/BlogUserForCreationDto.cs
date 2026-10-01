namespace DriversBlogManagement.Domain.BlogUsers.Dtos;

using Destructurama.Attributed;

public sealed record BlogUserForCreationDto
{
    public string UserName { get; set; }
    public string Email { get; set; }

}
