namespace DiscreteMathToolkit.Core.NumberSystems;

public sealed class HammingResult
{
    public IReadOnlyList<int> Encoded { get; }
    public IReadOnlyList<string> ExplanationSteps { get; }

    public HammingResult(IReadOnlyList<int> encoded, IReadOnlyList<string> steps)
    {
        Encoded = encoded;
        ExplanationSteps = steps;
    }
}

public sealed class HammingDecodeResult
{
    public IReadOnlyList<int> Corrected { get; }
    public IReadOnlyList<int> Data { get; }
    public int? ErrorPosition { get; }
    public IReadOnlyList<string> ExplanationSteps { get; }

    public HammingDecodeResult(IReadOnlyList<int> corrected, IReadOnlyList<int> data, int? errorPos, IReadOnlyList<string> steps)
    {
        Corrected = corrected;
        Data = data;
        ErrorPosition = errorPos;
        ExplanationSteps = steps;
    }
}

/// <summary>
/// Parity and Hamming(7,4) routines. Bit indexing uses 1-based positions in the textbook layout:
///   pos 1 = p1, pos 2 = p2, pos 3 = d1, pos 4 = p3, pos 5 = d2, pos 6 = d3, pos 7 = d4.
/// </summary>
public static class ErrorCorrectingCodes
{
    public static int EvenParityBit(IEnumerable<int> bits) =>
        bits.Sum() % 2 == 0 ? 0 : 1;

    public static int OddParityBit(IEnumerable<int> bits) =>
        bits.Sum() % 2 == 0 ? 1 : 0;

    public static HammingResult EncodeHamming74(IReadOnlyList<int> data4)
    {
        if (data4 is null || data4.Count != 4)
            throw new ArgumentException("Hamming(7,4) requires exactly 4 data bits.", nameof(data4));
        for (int i = 0; i < 4; i++)
            if (data4[i] != 0 && data4[i] != 1)
                throw new ArgumentException("Data bits must be 0 or 1.", nameof(data4));

        int d1 = data4[0], d2 = data4[1], d3 = data4[2], d4 = data4[3];
        int p1 = d1 ^ d2 ^ d4;       // covers positions 1,3,5,7 → d1, d2, d4
        int p2 = d1 ^ d3 ^ d4;       // covers positions 2,3,6,7 → d1, d3, d4
        int p3 = d2 ^ d3 ^ d4;       // covers positions 4,5,6,7 → d2, d3, d4

        var encoded = new[] { p1, p2, d1, p3, d2, d3, d4 };
        var steps = new List<string>
        {
            $"Data bits d1 d2 d3 d4 = {d1} {d2} {d3} {d4}.",
            $"Parity p1 (covers d1, d2, d4) = {d1} XOR {d2} XOR {d4} = {p1}.",
            $"Parity p2 (covers d1, d3, d4) = {d1} XOR {d3} XOR {d4} = {p2}.",
            $"Parity p3 (covers d2, d3, d4) = {d2} XOR {d3} XOR {d4} = {p3}.",
            $"Final code word [p1 p2 d1 p3 d2 d3 d4] = [{string.Join(' ', encoded)}]."
        };
        return new HammingResult(encoded, steps);
    }

    public static HammingDecodeResult DecodeHamming74(IReadOnlyList<int> received7)
    {
        if (received7 is null || received7.Count != 7)
            throw new ArgumentException("Hamming(7,4) decode requires exactly 7 bits.", nameof(received7));
        for (int i = 0; i < 7; i++)
            if (received7[i] != 0 && received7[i] != 1)
                throw new ArgumentException("Bits must be 0 or 1.", nameof(received7));

        // syndrome bits c1 c2 c3 (positions covered: c1 -> 1,3,5,7; c2 -> 2,3,6,7; c3 -> 4,5,6,7)
        int c1 = received7[0] ^ received7[2] ^ received7[4] ^ received7[6];
        int c2 = received7[1] ^ received7[2] ^ received7[5] ^ received7[6];
        int c3 = received7[3] ^ received7[4] ^ received7[5] ^ received7[6];
        int syndrome = (c3 << 2) | (c2 << 1) | c1;     // bit position (1-based) of error, or 0 if no error

        var corrected = received7.ToArray();
        int? errorPos = null;
        var steps = new List<string>
        {
            $"Received word: [{string.Join(' ', received7)}].",
            $"Check c1 (positions 1,3,5,7) = {c1}.",
            $"Check c2 (positions 2,3,6,7) = {c2}.",
            $"Check c3 (positions 4,5,6,7) = {c3}.",
            $"Syndrome (binary c3 c2 c1) = {c3}{c2}{c1} = {syndrome} (decimal)."
        };
        if (syndrome == 0)
        {
            steps.Add("Syndrome is zero → no error detected.");
        }
        else
        {
            errorPos = syndrome;
            corrected[syndrome - 1] ^= 1;
            steps.Add($"Syndrome non-zero → error at bit position {syndrome}; flip and correct.");
            steps.Add($"Corrected word: [{string.Join(' ', corrected)}].");
        }

        var data = new[] { corrected[2], corrected[4], corrected[5], corrected[6] }; // d1 d2 d3 d4
        steps.Add($"Extracted data bits d1 d2 d3 d4 = [{string.Join(' ', data)}].");
        return new HammingDecodeResult(corrected, data, errorPos, steps);
    }
}
