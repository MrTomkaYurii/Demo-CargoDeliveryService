namespace CargoOrderingService.Domain.Users.Models;

using Destructurama.Attributed;

public sealed record UserForUpdate
{
    public string Identifier { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }

}
