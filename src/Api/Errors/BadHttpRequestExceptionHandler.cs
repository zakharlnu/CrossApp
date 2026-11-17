using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

public sealed class BadHttpRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        await Results.Problem(
                statusCode: badRequest.StatusCode,
                title: "Некоректний запит",
                detail: "Перевірте формат JSON і типи переданих полів.")
            .ExecuteAsync(httpContext);

        return true;
    }
}
