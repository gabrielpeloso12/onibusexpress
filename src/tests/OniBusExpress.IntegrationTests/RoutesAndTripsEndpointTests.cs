using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OniBusExpress.Application.DTOs;

namespace OniBusExpress.IntegrationTests;

public class RoutesAndTripsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoutesAndTripsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoutes_ReturnsSeededRoutes()
    {
        var routes = await _client.GetFromJsonAsync<List<RouteDto>>("/rotas");

        routes.Should().NotBeNull();
        routes!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchTrips_ReturnsResults_WithoutAnyFilter()
    {
        var trips = await _client.GetFromJsonAsync<List<TripSummaryDto>>("/viagens");

        trips.Should().NotBeNull();
        trips!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchTrips_FiltersByOrigin()
    {
        var trips = await _client.GetFromJsonAsync<List<TripSummaryDto>>("/viagens?origem=Paulo");

        trips.Should().NotBeNull();
        trips!.Should().OnlyContain(t => t.Origin.Contains("Paulo", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTripById_ReturnsSeatMap_WhenTripExists()
    {
        var trips = await _client.GetFromJsonAsync<List<TripSummaryDto>>("/viagens");
        var tripId = trips!.First().Id;

        var response = await _client.GetAsync($"/viagens/{tripId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var details = await response.Content.ReadFromJsonAsync<TripDetailsDto>();
        details.Should().NotBeNull();
        details!.Seats.Should().HaveCount(details.TotalSeats);
    }

    [Fact]
    public async Task GetTripById_ReturnsNotFound_WhenTripDoesNotExist()
    {
        var response = await _client.GetAsync($"/viagens/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
