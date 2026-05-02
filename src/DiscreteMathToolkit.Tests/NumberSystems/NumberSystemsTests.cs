using DiscreteMathToolkit.Core.NumberSystems;
using FluentAssertions;
using Xunit;

namespace DiscreteMathToolkit.Tests.NumberSystems;

public class NumberSystemsTests
{
    [Theory]
    [InlineData("255", 10, 2, "11111111")]
    [InlineData("1010", 2, 10, "10")]
    [InlineData("FF", 16, 10, "255")]
    [InlineData("100", 10, 16, "64")]
    [InlineData("777", 8, 10, "511")]
    [InlineData("0", 10, 2, "0")]
    public void BaseConverter_KnownValues(string input, int from, int to, string expected)
    {
        var result = BaseConverter.Convert(input, from, to);
        result.Output.Should().Be(expected);
    }

    [Fact]
    public void BaseConverter_HandlesNegativeNumbers()
    {
        var result = BaseConverter.Convert("-10", 10, 2);
        result.Output.Should().Be("-1010");
    }

    [Fact]
    public void BaseConverter_RecordsExplanationSteps()
    {
        var result = BaseConverter.Convert("255", 10, 16);
        result.Output.Should().Be("FF");
        result.Steps.Should().NotBeEmpty();
    }

    [Fact]
    public void BaseConverter_RejectsInvalidDigit()
    {
        Assert.Throws<FormatException>(() => BaseConverter.Convert("9", 8, 10));
    }

    [Fact]
    public void BaseConverter_RejectsBadBase()
    {
        Assert.Throws<ArgumentException>(() => BaseConverter.Convert("1", 1, 10));
        Assert.Throws<ArgumentException>(() => BaseConverter.Convert("1", 10, 37));
    }

    [Theory]
    [InlineData(5, 8, "00000101")]
    [InlineData(-1, 8, "11111111")]
    [InlineData(-128, 8, "10000000")]
    [InlineData(127, 8, "01111111")]
    [InlineData(0, 4, "0000")]
    public void TwosComplement_KnownValues(long value, int bits, string expected)
    {
        BaseConverter.ToTwosComplement(value, bits).Should().Be(expected);
    }

    [Fact]
    public void TwosComplement_OutOfRangeThrows()
    {
        Assert.Throws<ArgumentException>(() => BaseConverter.ToTwosComplement(128, 8));
        Assert.Throws<ArgumentException>(() => BaseConverter.ToTwosComplement(-129, 8));
    }

    [Fact]
    public void EvenParity_FlipsToMakeSumEven()
    {
        ErrorCorrectingCodes.EvenParityBit(new[] { 1, 0, 1 }).Should().Be(0);
        ErrorCorrectingCodes.EvenParityBit(new[] { 1, 0, 0 }).Should().Be(1);
    }

    [Fact]
    public void OddParity_FlipsToMakeSumOdd()
    {
        ErrorCorrectingCodes.OddParityBit(new[] { 1, 0, 0 }).Should().Be(0);
        ErrorCorrectingCodes.OddParityBit(new[] { 1, 1, 0 }).Should().Be(1);
    }

    [Fact]
    public void Hamming74_EncodeThenDecode_RecoversOriginalData()
    {
        var data = new[] { 1, 0, 1, 1 };
        var encoded = ErrorCorrectingCodes.EncodeHamming74(data);
        var decoded = ErrorCorrectingCodes.DecodeHamming74(encoded.Encoded);
        decoded.ErrorPosition.Should().BeNull();
        decoded.Data.Should().Equal(data);
    }

    [Fact]
    public void Hamming74_DetectsAndCorrectsSingleBitError()
    {
        var data = new[] { 1, 0, 1, 1 };
        var encoded = ErrorCorrectingCodes.EncodeHamming74(data).Encoded.ToArray();
        // flip bit at position 4 (index 3)
        encoded[3] ^= 1;
        var decoded = ErrorCorrectingCodes.DecodeHamming74(encoded);
        decoded.ErrorPosition.Should().Be(4);
        decoded.Data.Should().Equal(data);
    }

    [Fact]
    public void Hamming74_RejectsWrongLength()
    {
        Assert.Throws<ArgumentException>(() => ErrorCorrectingCodes.EncodeHamming74(new[] { 1, 0, 1 }));
        Assert.Throws<ArgumentException>(() => ErrorCorrectingCodes.DecodeHamming74(new[] { 1, 0, 1 }));
    }
}
