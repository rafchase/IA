using CpfApi.Validators;
using Xunit;

namespace CpfApi.Tests.Validators;

public class CpfValidatorTests
{
    // --- Validate ---

    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void Validate_ValidCpf_ReturnsTrue(string cpf)
    {
        Assert.True(CpfValidator.Validate(cpf));
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000")]
    [InlineData("99999999999")]
    public void Validate_AllSameDigits_ReturnsFalse(string cpf)
    {
        Assert.False(CpfValidator.Validate(cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567890")]   // 10 digits
    [InlineData("123456789012")] // 12 digits
    public void Validate_WrongLength_ReturnsFalse(string cpf)
    {
        Assert.False(CpfValidator.Validate(cpf));
    }

    [Theory]
    [InlineData("529.982.247-26")] // last digit wrong
    [InlineData("529.982.247-35")] // both wrong
    [InlineData("52998224724")]    // first check digit wrong
    public void Validate_InvalidCheckDigit_ReturnsFalse(string cpf)
    {
        Assert.False(CpfValidator.Validate(cpf));
    }

    // --- Strip ---

    [Fact]
    public void Strip_FormattedCpf_ReturnsDigitsOnly()
    {
        Assert.Equal("52998224725", CpfValidator.Strip("529.982.247-25"));
    }

    [Fact]
    public void Strip_AlreadyStripped_ReturnsSameValue()
    {
        Assert.Equal("52998224725", CpfValidator.Strip("52998224725"));
    }

    // --- Format ---

    [Fact]
    public void Format_RawDigits_ReturnsFormatted()
    {
        Assert.Equal("529.982.247-25", CpfValidator.Format("52998224725"));
    }

    [Fact]
    public void Format_AlreadyFormatted_ReturnsFormatted()
    {
        Assert.Equal("529.982.247-25", CpfValidator.Format("529.982.247-25"));
    }

    [Fact]
    public void Format_WrongLength_ReturnsInputUnchanged()
    {
        Assert.Equal("123", CpfValidator.Format("123"));
    }
}
