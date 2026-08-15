using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Enums;
using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.UnitTests.Entities;

public class BookingCancellationTests
{
    [Fact]
    public void Cancel_Succeeds_WhenMoreThanTwoHoursBeforeDeparture()
    {
        var now = DateTime.UtcNow;
        var departure = now.AddHours(3);
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "ABC-12345", now.AddDays(-1));

        booking.Cancel(departure, now);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(now, booking.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_Throws_WhenLessThanTwoHoursBeforeDeparture()
    {
        var now = DateTime.UtcNow;
        var departure = now.AddHours(1);
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "ABC-12345", now.AddDays(-1));

        Assert.Throws<CancellationWindowExpiredException>(() => booking.Cancel(departure, now));
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Cancel_Succeeds_WhenExactlyAtTwoHourBoundary()
    {
        // "Até 2 horas antes da partida" é inclusivo: com exatamente 2h de antecedência ainda é cancelável.
        var now = DateTime.UtcNow;
        var departure = now.AddHours(2);
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "ABC-12345", now.AddDays(-1));

        booking.Cancel(departure, now);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_Throws_WhenOneMinuteInsideTwoHourBoundary()
    {
        var now = DateTime.UtcNow;
        var departure = now.AddHours(2).AddMinutes(-1);
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "ABC-12345", now.AddDays(-1));

        Assert.Throws<CancellationWindowExpiredException>(() => booking.Cancel(departure, now));
    }

    [Fact]
    public void Cancel_Throws_WhenAlreadyCancelled()
    {
        var now = DateTime.UtcNow;
        var departure = now.AddDays(1);
        var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "ABC-12345", now.AddDays(-1));
        booking.Cancel(departure, now);

        Assert.Throws<BookingAlreadyCancelledException>(() => booking.Cancel(departure, now));
    }
}
