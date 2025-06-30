using ManutencaoPreditiva.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Application.Common
{
    public static class ResponseHandler
    {
        public static IActionResult HandlerResult<T>(Result<T> result)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => new NotFoundObjectResult(new { result.ErrorMessage }),
                ResultErrorType.Conflict => new ConflictObjectResult(new { result.ErrorMessage }),
                ResultErrorType.Invalid => new BadRequestObjectResult(new { result.ErrorMessage }),
                _ => new ObjectResult(new { result.ErrorMessage }) { StatusCode = 500 }
            };
        }
    }
}
