namespace OniBusExpress.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
