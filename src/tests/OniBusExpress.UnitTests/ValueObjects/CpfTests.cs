using OniBusExpress.Domain.Exceptions;
using OniBusExpress.Domain.ValueObjects;

namespace OniBusExpress.UnitTests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void IsValid_ReturnsTrue_ForValidCpf(string cpf)
    {
        Assert.True(Cpf.IsValid(cpf));
    }

    [Theory]
    [InlineData("529.982.247-24")] // dígito verificador errado
    [InlineData("00000000000")]    // dígitos repetidos
    [InlineData("11111111111")]    // dígitos repetidos
    [InlineData("123456789")]      // curto demais
    [InlineData("123456789012")]   // longo demais
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_ReturnsFalse_ForInvalidCpf(string? cpf)
    {
        Assert.False(Cpf.IsValid(cpf));
    }

    [Fact]
    public void Create_ReturnsCpfWithNormalizedDigits_WhenValid()
    {
        var cpf = Cpf.Create("529.982.247-25");

        Assert.Equal("52998224725", cpf.Value);
        Assert.Equal("529.982.247-25", cpf.ToFormattedString());
    }

    [Fact]
    public void Create_Throws_WhenInvalid()
    {
        Assert.Throws<InvalidCpfException>(() => Cpf.Create("111.111.111-11"));
    }

    [Fact]
    public void TwoCpfInstances_WithSameDigits_AreEqual()
    {
        var a = Cpf.Create("529.982.247-25");
        var b = Cpf.Create("52998224725");

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }
}
