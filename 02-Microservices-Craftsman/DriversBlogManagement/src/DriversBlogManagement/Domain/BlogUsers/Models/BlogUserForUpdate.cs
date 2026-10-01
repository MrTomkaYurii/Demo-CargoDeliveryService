namespace DriversBlogManagement.Domain.BlogUsers.Models;

using Destructurama.Attributed;

public sealed class BlogUserForUpdate
{
    public string UserName { get; set; }
    public string Email { get; set; }

}
