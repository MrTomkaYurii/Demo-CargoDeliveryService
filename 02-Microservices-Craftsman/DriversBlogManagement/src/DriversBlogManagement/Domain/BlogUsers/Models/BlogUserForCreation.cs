namespace DriversBlogManagement.Domain.BlogUsers.Models;

using Destructurama.Attributed;

public sealed class BlogUserForCreation
{
    public string UserName { get; set; }
    public string Email { get; set; }

}
