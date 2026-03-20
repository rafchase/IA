using CpfApi.Validators;
using Xunit;

namespace CpfApi.Tests.Validators;

public class CnpjValidatorTests
{
    // --- Validate ---

    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    [InlineData("12345678000195")]
    public void Validate_ValidCnpj_ReturnsTrue(string cnpj)
    {
        var digits = CnpjValidator.Strip(cnpj);
        Assert.True(CnpjValidator.Validate(digits));
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    [InlineData("99999999999999")]
    public void Validate_AllSameDigits_ReturnsFalse(string cnpj)
    {
        Assert.False(CnpjValidator.Validate(cnpj));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123")]   // 13 digits
    [InlineData("123456789012345")] // 15 digits
    public void Validate_WrongLength_ReturnsFalse(string cnpj)
    {
        Assert.False(CnpjValidator.Validate(cnpj));
    }

    [Theory]
    [InlineData("11222333000182")] // last digit wrong
    [InlineData("11222333000191")] // first check digit wrong
    public void Validate_InvalidCheckDigit_ReturnsFalse(string cnpj)
    {
        Assert.False(CnpjValidator.Validate(cnpj));
    }

    // --- Strip ---

    [Fact]
    public void Strip_FormattedCnpj_ReturnsDigitsOnly()
    {
        Assert.Equal("11222333000181", CnpjValidator.Strip("11.222.333/0001-81"));
    }

    [Fact]
    public void Strip_AlreadyStripped_ReturnsSameValue()
    {
        Assert.Equal("11222333000181", CnpjValidator.Strip("11222333000181"));
    }

    // --- Format ---

    [Fact]
    public void Format_RawDigits_ReturnsFormatted()
    {
        Assert.Equal("11.222.333/0001-81", CnpjValidator.Format("11222333000181"));
    }

    [Fact]
    public void Format_WrongLength_ReturnsInputUnchanged()
    {
        Assert.Equal("123", CnpjValidator.Format("123"));
    }
}
