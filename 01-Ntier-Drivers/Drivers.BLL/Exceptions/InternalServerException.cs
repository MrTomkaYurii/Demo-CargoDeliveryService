using System.Net;

namespace Drivers.BLL.Exceptions
{
    public class InternalServerException : CustomException
    {
        public InternalServerException(string message,
            List<string>? errors = default)
            : base(message,
                  errors,
                  HttpStatusCode.InternalServerError)
        { }
    }
}
