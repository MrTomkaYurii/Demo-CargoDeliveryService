using System.Net;

namespace Drivers.BLL.Exceptions
{
    public class NotFoundException : CustomException
    {
        public NotFoundException(string message)
            : base(message,
                  null,
                  HttpStatusCode.NotFound)
        {
        }
    }
}
