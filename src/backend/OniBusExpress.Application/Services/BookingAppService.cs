using OniBusExpress.Application.Abstractions;
using OniBusExpress.Application.DTOs;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Exceptions;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Domain.ValueObjects;

namespace OniBusExpress.Application.Services;

public sealed class BookingAppService : IBookingAppService
{
    private readonly ITripRepository _tripRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservationCodeGenerator _codeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BookingAppService(
        ITripRepository tripRepository,
        IPassengerRepository passengerRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IReservationCodeGenerator codeGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _tripRepository = tripRepository;
        _passengerRepository = passengerRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _codeGenerator = codeGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<BookingResponseDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var cpf = Cpf.Create(request.PassengerCpf);
        var now = _dateTimeProvider.UtcNow;

        var trip = await _tripRepository.GetByIdWithBookingsAsync(request.TripId, cancellationToken)
                   ?? throw new TripNotFoundException(request.TripId);

        trip.EnsureCanBeBooked(request.SeatNumber, now);

        var passenger = await _passengerRepository.GetByCpfAsync(cpf.Value, cancellationToken);
        if (passenger is null)
        {
            passenger = new Passenger(request.PassengerName, cpf, request.PassengerEmail, request.PassengerBirthDate);
            await _passengerRepository.AddAsync(passenger, cancellationToken);
        }

        var reservationCode = await _codeGenerator.GenerateUniqueCodeAsync(cancellationToken);
        var booking = Booking.Create(trip.Id, passenger.Id, request.SeatNumber, reservationCode, now);

        await _bookingRepository.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(booking, trip, passenger);
    }

    public async Task<BookingResponseDto?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByCodeAsync(NormalizeCode(reservationCode), cancellationToken);
        return booking is null ? null : ToDto(booking, booking.Trip!, booking.Passenger!);
    }

    public async Task<bool> CancelAsync(string reservationCode, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByCodeAsync(NormalizeCode(reservationCode), cancellationToken);
        if (booking is null)
            return false;

        var now = _dateTimeProvider.UtcNow;
        booking.Cancel(booking.Trip!.DepartureDateTime, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NormalizeCode(string reservationCode) => reservationCode.Trim().ToUpperInvariant();

    private static BookingResponseDto ToDto(Booking booking, Trip trip, Passenger passenger) => new(
        booking.ReservationCode,
        booking.Status.ToString(),
        trip.Id,
        trip.Route!.Origin,
        trip.Route!.Destination,
        trip.DepartureDateTime,
        trip.BasePrice,
        booking.SeatNumber,
        passenger.Name,
        passenger.Cpf.ToFormattedString(),
        passenger.Email,
        booking.CreatedAtUtc,
        booking.CancelledAtUtc);
}
