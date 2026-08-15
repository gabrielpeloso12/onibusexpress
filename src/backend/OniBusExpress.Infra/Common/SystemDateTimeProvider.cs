using OniBusExpress.Application.Abstractions;

namespace OniBusExpress.Infra.Common;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
