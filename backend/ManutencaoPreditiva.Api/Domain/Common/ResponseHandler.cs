using ManutencaoPreditiva.Api.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Domain.Common;

public static class ResponseHandler
{
    public static IActionResult HandlerResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return result.ErrorType switch
        {
            Result<T>.ResultErrorType.NotFound => new NotFoundObjectResult(result.ErrorMessage),
            Result<T>.ResultErrorType.Unauthorized => new UnauthorizedObjectResult(result.ErrorMessage),
            Result<T>.ResultErrorType.BadRequest => new BadRequestObjectResult(result.ErrorMessage),
            Result<T>.ResultErrorType.NoContent => new NoContentResult(),
            Result<T>.ResultErrorType.ServiceUnavailable => new ObjectResult(result.ErrorMessage) { StatusCode = 500 },
            _ => new ObjectResult(result.ErrorMessage) { StatusCode = 500 }
        };
    }
}
