using System.Text.RegularExpressions;
using Moq;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Services;

namespace OniBusExpress.UnitTests.Services;

public partial class ReservationCodeGeneratorTests
{
    [Fact]
    public async Task GenerateUniqueCodeAsync_ReturnsCodeInExpectedFormat()
    {
        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var generator = new ReservationCodeGenerator(bookingRepository.Object);

        var code = await generator.GenerateUniqueCodeAsync();

        Assert.Matches(CodePattern(), code);
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_RetriesUntilAnUnusedCodeIsFound()
    {
        var bookingRepository = new Mock<IBookingRepository>();
        var callCount = 0;

        bookingRepository
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++callCount < 3); // os dois primeiros "candidatos" colidem, o terceiro está livre

        var generator = new ReservationCodeGenerator(bookingRepository.Object);

        var code = await generator.GenerateUniqueCodeAsync();

        Assert.Matches(CodePattern(), code);
        bookingRepository.Verify(
            r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Throws_WhenNoUniqueCodeIsFoundWithinMaxAttempts()
    {
        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository
            .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var generator = new ReservationCodeGenerator(bookingRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateUniqueCodeAsync());
    }

    [GeneratedRegex(@"^[A-Z]{3}-\d{5}$")]
    private static partial Regex CodePattern();
}
