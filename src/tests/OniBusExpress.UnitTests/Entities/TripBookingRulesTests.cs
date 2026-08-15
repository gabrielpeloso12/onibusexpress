using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.UnitTests.Entities;

public class TripBookingRulesTests
{
    private static Trip CreateFutureTrip(int totalSeats = 10) =>
        new(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 100m, totalSeats);

    [Fact]
    public void EnsureCanBeBooked_DoesNotThrow_WhenSeatIsFreeAndTripIsInTheFuture()
    {
        var trip = CreateFutureTrip();

        var exception = Record.Exception(() => trip.EnsureCanBeBooked(1, DateTime.UtcNow));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanBeBooked_Throws_WhenSeatIsAlreadyOccupied()
    {
        var trip = CreateFutureTrip();
        var now = DateTime.UtcNow;

        var existingBooking = Booking.Create(trip.Id, Guid.NewGuid(), seatNumber: 5, reservationCode: "ABC-11111", now);
        trip.AttachExistingBooking(existingBooking);

        Assert.Throws<SeatAlreadyBookedException>(() => trip.EnsureCanBeBooked(5, now));
    }

    [Fact]
    public void EnsureCanBeBooked_DoesNotThrow_WhenSeatWasBookedThenCancelled()
    {
        var trip = CreateFutureTrip();
        var now = DateTime.UtcNow;

        var cancelledBooking = Booking.Create(trip.Id, Guid.NewGuid(), seatNumber: 5, reservationCode: "ABC-11111", now.AddDays(-1));
        cancelledBooking.Cancel(trip.DepartureDateTime, now.AddDays(-1));
        trip.AttachExistingBooking(cancelledBooking);

        var exception = Record.Exception(() => trip.EnsureCanBeBooked(5, now));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanBeBooked_Throws_WhenTripHasAlreadyDeparted()
    {
        var trip = new Trip(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1), 100m, 10);

        Assert.Throws<TripAlreadyDepartedException>(() => trip.EnsureCanBeBooked(1, DateTime.UtcNow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void EnsureCanBeBooked_Throws_WhenSeatNumberIsOutOfRange(int seatNumber)
    {
        var trip = CreateFutureTrip(totalSeats: 10);

        Assert.Throws<InvalidSeatNumberException>(() => trip.EnsureCanBeBooked(seatNumber, DateTime.UtcNow));
    }
}
