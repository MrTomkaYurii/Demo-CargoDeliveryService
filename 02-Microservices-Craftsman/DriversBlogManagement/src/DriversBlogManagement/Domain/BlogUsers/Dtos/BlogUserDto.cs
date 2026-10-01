namespace DriversBlogManagement.Domain.BlogUsers.Dtos;

using Destructurama.Attributed;

public sealed record BlogUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

}
