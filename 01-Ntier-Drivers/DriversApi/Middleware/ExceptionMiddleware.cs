using Drivers.BLL.Exceptions;
using Drivers.BLL.Exceptions.Models;
using Drivers.DAL.EF.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Drivers.Api.Middleware
{
    public class ExceptionMiddleware : IMiddleware
    {

        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) => _logger = logger;  
      

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                string errorId = Guid.NewGuid().ToString();
                _logger.LogError($"==>> ErrorId -  {errorId}");
                _logger.LogError($"==>> StackTrace- {exception.StackTrace}");
                var errorResult = new ErrorResult
                {
                    Source = exception.TargetSite?.DeclaringType?.FullName,
                    Exception = exception.Message.Trim(),
                    ErrorId = errorId,
                    SupportMessage = $"Provide the Error Id: {errorId} to the support team for further analysis."
                };
                errorResult.Messages.Add(exception.Message);

                //if (exception is not CustomException && exception.InnerException != null)
                //{
                //    while (exception.InnerException != null)
                //    {
                //        exception = exception.InnerException;
                //    }
                //}

                switch (exception)
                {
                    case CustomException e:
                        errorResult.StatusCode = (int)e.StatusCode;
                        if (e.ErrorMessages is not null)
                        {
                            errorResult.Messages = e.ErrorMessages;
                        }

                        break;

                    case KeyNotFoundException or EntityNotFoundException:
                        errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    case ArgumentNullException:
                        errorResult.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    default:
                        errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.LogError($"==>> {errorResult.Exception} Request failed with Status Code {context.Response.StatusCode} and Error Id {errorId}.");
                var response = context.Response;
                
                if (!response.HasStarted)
                {
                    response.ContentType = "application/json";
                    response.StatusCode = errorResult.StatusCode;
                    await response.WriteAsync(JsonConvert.SerializeObject(errorResult));
                }
                else
                {
                    _logger.LogWarning(" ==>> Can't write error response. Response has already started.");
                }
            }
        }
    }
}