using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OniBusExpress.Application.DTOs;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.IntegrationTests;

public class BookingsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidCpf = "52998224725";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BookingsEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var tripId = await GetAnyFutureTripIdAsync();

        var response = await _client.PostAsJsonAsync("/reservas", BuildValidRequest(tripId, seatNumber: 2));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        booking.Should().NotBeNull();
        booking!.ReservationCode.Should().MatchRegex("^[A-Z]{3}-\\d{5}$");
        booking.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenSeatIsAlreadyBooked()
    {
        var tripId = await GetAnyFutureTripIdAsync();

        var first = await _client.PostAsJsonAsync("/reservas", BuildValidRequest(tripId, seatNumber: 3));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync(
            "/reservas",
            BuildValidRequest(tripId, seatNumber: 3, cpf: "11144477735"));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenCpfIsInvalid()
    {
        var tripId = await GetAnyFutureTripIdAsync();

        var response = await _client.PostAsJsonAsync(
            "/reservas",
            BuildValidRequest(tripId, seatNumber: 4, cpf: "11111111111"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenTripDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/reservas", BuildValidRequest(Guid.NewGuid(), seatNumber: 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCode_ReturnsBooking_WhenItExists()
    {
        var tripId = await GetAnyFutureTripIdAsync();
        var created = await CreateBookingAsync(tripId, seatNumber: 6);

        var response = await _client.GetAsync($"/reservas/{created.ReservationCode}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        booking!.ReservationCode.Should().Be(created.ReservationCode);
    }

    [Fact]
    public async Task GetByCode_ReturnsNotFound_WhenCodeDoesNotExist()
    {
        var response = await _client.GetAsync("/reservas/ZZZ-99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cancel_ReturnsNoContent_WhenWellBeforeDeparture()
    {
        var tripId = await GetAnyFutureTripIdAsync();
        var created = await CreateBookingAsync(tripId, seatNumber: 7);

        var response = await _client.DeleteAsync($"/reservas/{created.ReservationCode}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterCancel = await _client.GetFromJsonAsync<BookingResponseDto>($"/reservas/{created.ReservationCode}");
        afterCancel!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Cancel_ReturnsConflict_WhenLessThanTwoHoursBeforeDeparture()
    {
        var tripId = await CreateTripDepartingSoonAsync();
        var created = await CreateBookingAsync(tripId, seatNumber: 1);

        var response = await _client.DeleteAsync($"/reservas/{created.ReservationCode}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cancel_ReturnsNotFound_WhenCodeDoesNotExist()
    {
        var response = await _client.DeleteAsync("/reservas/ZZZ-99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> GetAnyFutureTripIdAsync()
    {
        var trips = await _client.GetFromJsonAsync<List<TripSummaryDto>>("/viagens");
        return trips!.First().Id;
    }

    private async Task<Guid> CreateTripDepartingSoonAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OniBusExpressDbContext>();

        var route = await dbContext.Routes.FirstAsync();
        var trip = new Trip(route.Id, DateTime.UtcNow.AddHours(1), 75m, 10);

        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        return trip.Id;
    }

    private async Task<BookingResponseDto> CreateBookingAsync(Guid tripId, int seatNumber, string cpf = ValidCpf)
    {
        var response = await _client.PostAsJsonAsync("/reservas", BuildValidRequest(tripId, seatNumber, cpf));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookingResponseDto>())!;
    }

    private static CreateBookingRequest BuildValidRequest(Guid tripId, int seatNumber, string cpf = ValidCpf) => new()
    {
        PassengerName = "Maria Teste",
        PassengerCpf = cpf,
        PassengerEmail = "maria@example.com",
        PassengerBirthDate = new DateOnly(1990, 1, 1),
        TripId = tripId,
        SeatNumber = seatNumber
    };
}
