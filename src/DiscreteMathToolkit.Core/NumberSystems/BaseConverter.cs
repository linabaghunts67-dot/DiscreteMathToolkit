using System.Globalization;
using System.Text;

namespace DiscreteMathToolkit.Core.NumberSystems;

public sealed class ConversionStep
{
    public string Description { get; }
    public string Detail { get; }
    public ConversionStep(string desc, string detail) { Description = desc; Detail = detail; }
}

public sealed class ConversionResult
{
    public long Value { get; }
    public int FromBase { get; }
    public int ToBase { get; }
    public string Output { get; }
    public IReadOnlyList<ConversionStep> Steps { get; }

    public ConversionResult(long value, int fromBase, int toBase, string output, IReadOnlyList<ConversionStep> steps)
    {
        Value = value;
        FromBase = fromBase;
        ToBase = toBase;
        Output = output;
        Steps = steps;
    }
}

public static class BaseConverter
{
    private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static ConversionResult Convert(string input, int fromBase, int toBase)
    {
        if (fromBase < 2 || fromBase > 36) throw new ArgumentException("fromBase must be in [2, 36].", nameof(fromBase));
        if (toBase < 2 || toBase > 36) throw new ArgumentException("toBase must be in [2, 36].", nameof(toBase));
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Input is empty.", nameof(input));

        var steps = new List<ConversionStep>();
        bool negative = false;
        string body = input.Trim();
        if (body.StartsWith('-')) { negative = true; body = body.Substring(1); }
        body = body.ToUpperInvariant();

        // Parse from source base
        long value = 0;
        steps.Add(new ConversionStep(
            $"Parse '{body}' as base {fromBase}.",
            ExplainParse(body, fromBase)));
        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            int digit = Digits.IndexOf(c);
            if (digit < 0 || digit >= fromBase)
                throw new FormatException($"Character '{c}' is not a valid base-{fromBase} digit.");
            value = checked(value * fromBase + digit);
        }

        steps.Add(new ConversionStep(
            $"Decimal value = {(negative ? -value : value)}.",
            $"Sum of (digit × {fromBase}^position) yields {value}."));

        long absValue = value;
        // Convert to target base
        string output;
        if (absValue == 0)
        {
            output = "0";
            steps.Add(new ConversionStep("Value is 0 → output '0'.", string.Empty));
        }
        else
        {
            var sb = new StringBuilder();
            var detail = new StringBuilder();
            long current = absValue;
            while (current > 0)
            {
                long rem = current % toBase;
                long quot = current / toBase;
                detail.AppendLine($"  {current} ÷ {toBase} = {quot} remainder {rem}  → digit '{Digits[(int)rem]}'");
                sb.Insert(0, Digits[(int)rem]);
                current = quot;
            }
            output = sb.ToString();
            steps.Add(new ConversionStep(
                $"Repeatedly divide by {toBase}; collect remainders bottom-up.",
                detail.ToString().TrimEnd()));
        }

        if (negative) output = "-" + output;
        return new ConversionResult(negative ? -value : value, fromBase, toBase, output, steps);
    }

    /// <summary>Two's-complement representation of a signed integer in <paramref name="bits"/> bits.</summary>
    public static string ToTwosComplement(long value, int bits)
    {
        if (bits is < 2 or > 64) throw new ArgumentException("Bit width must be in [2, 64].", nameof(bits));
        long min = bits == 64 ? long.MinValue : -(1L << (bits - 1));
        long max = bits == 64 ? long.MaxValue : (1L << (bits - 1)) - 1;
        if (value < min || value > max)
            throw new ArgumentException($"Value {value} does not fit in {bits} bits (range [{min}, {max}]).", nameof(value));

        // Take exactly the low `bits` bits of the two's-complement representation.
        ulong mask = bits == 64 ? ulong.MaxValue : (1UL << bits) - 1;
        ulong unsigned = (ulong)value & mask;
        var chars = new char[bits];
        for (int i = 0; i < bits; i++)
            chars[bits - 1 - i] = ((unsigned >> i) & 1UL) == 1UL ? '1' : '0';
        return new string(chars);
    }

    private static string ExplainParse(string body, int fromBase)
    {
        var sb = new StringBuilder();
        int len = body.Length;
        long running = 0;
        for (int i = 0; i < len; i++)
        {
            int digit = Digits.IndexOf(body[i]);
            int power = len - 1 - i;
            running = running * fromBase + digit;
            sb.Append(CultureInfo.InvariantCulture, $"  '{body[i]}' × {fromBase}^{power} = {digit * Math.Pow(fromBase, power):0}");
            if (i < len - 1) sb.AppendLine();
        }
        sb.AppendLine();
        sb.Append(CultureInfo.InvariantCulture, $"  Total = {running}");
        return sb.ToString();
    }
}
