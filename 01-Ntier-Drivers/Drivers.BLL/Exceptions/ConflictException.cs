using System.Net;

namespace Drivers.BLL.Exceptions
{
    public class ConflictException : CustomException
    {
        public ConflictException(string message)
            : base(message,
                  null,
                  HttpStatusCode.Conflict)
        { }
    }
}
