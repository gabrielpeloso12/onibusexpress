namespace OniBusExpress.Application.DTOs;

/// <summary>Payload de resposta de <c>GET /rotas</c>.</summary>
public sealed record RouteDto(
    Guid Id,
    string Origin,
    string Destination,
    TimeSpan EstimatedDuration);
