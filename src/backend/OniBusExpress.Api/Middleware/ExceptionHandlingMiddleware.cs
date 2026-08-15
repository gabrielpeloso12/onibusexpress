using System.Net;
using Microsoft.AspNetCore.Mvc;
using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.Api.Middleware;

/// <summary>Traduz exceções de domínio em respostas ProblemDetails com o código HTTP apropriado.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, DomainException exception)
    {
        var statusCode = MapToStatusCode(exception);

        _logger.LogInformation(exception, "Regra de negócio violada: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = statusCode switch
            {
                HttpStatusCode.NotFound => "Recurso não encontrado",
                HttpStatusCode.Conflict => "Conflito de regra de negócio",
                _ => "Requisição inválida"
            },
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static HttpStatusCode MapToStatusCode(DomainException exception) => exception switch
    {
        TripNotFoundException => HttpStatusCode.NotFound,
        SeatAlreadyBookedException => HttpStatusCode.Conflict,
        TripAlreadyDepartedException => HttpStatusCode.Conflict,
        CancellationWindowExpiredException => HttpStatusCode.Conflict,
        BookingAlreadyCancelledException => HttpStatusCode.Conflict,
        InvalidCpfException => HttpStatusCode.BadRequest,
        InvalidSeatNumberException => HttpStatusCode.BadRequest,
        _ => HttpStatusCode.BadRequest
    };
}
