using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.Core.NumberSystems;
using DiscreteMathToolkit.Infrastructure.Logging;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed partial class NumberSystemsViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAppLogger _logger;

    public string Title => "Number Systems";

    // Base conversion
    [ObservableProperty] private string _convertInput = "255";
    [ObservableProperty] private int _convertFromBase = 10;
    [ObservableProperty] private int _convertToBase = 2;
    [ObservableProperty] private string _convertOutput = string.Empty;
    [ObservableProperty] private string _statusLine = "Ready.";

    // Two's complement
    [ObservableProperty] private string _twosComplementInput = "-5";
    [ObservableProperty] private int _twosComplementBits = 8;
    [ObservableProperty] private string _twosComplementOutput = string.Empty;

    // Hamming encode
    [ObservableProperty] private string _hammingDataBits = "1 0 1 1";
    [ObservableProperty] private string _hammingEncodedBits = string.Empty;

    // Hamming decode
    [ObservableProperty] private string _hammingReceivedBits = "1 0 0 1 1 0 1";
    [ObservableProperty] private string _hammingDecodedData = string.Empty;
    [ObservableProperty] private string _hammingErrorReport = string.Empty;

    public ObservableCollection<int> AvailableBases { get; }
    public ObservableCollection<int> AvailableBitWidths { get; } = new(new[] { 4, 8, 16, 32, 64 });

    public ObservableCollection<string> ConversionStepDescriptions { get; } = new();
    public ObservableCollection<string> HammingEncodeSteps { get; } = new();
    public ObservableCollection<string> HammingDecodeSteps { get; } = new();

    public IRelayCommand ConvertBaseCommand { get; }
    public IRelayCommand ComputeTwosComplementCommand { get; }
    public IRelayCommand EncodeHammingCommand { get; }
    public IRelayCommand DecodeHammingCommand { get; }

    public NumberSystemsViewModel(IAppLogger logger)
    {
        _logger = logger;
        AvailableBases = new ObservableCollection<int>(Enumerable.Range(2, 35)); // 2..36

        ConvertBaseCommand = new RelayCommand(ConvertBase);
        ComputeTwosComplementCommand = new RelayCommand(ComputeTwosComplement);
        EncodeHammingCommand = new RelayCommand(EncodeHamming);
        DecodeHammingCommand = new RelayCommand(DecodeHamming);

        // populate on startup
        ConvertBase();
        ComputeTwosComplement();
        EncodeHamming();
        DecodeHamming();
    }

    private void ConvertBase()
    {
        try
        {
            var result = BaseConverter.Convert(ConvertInput, ConvertFromBase, ConvertToBase);
            ConvertOutput = result.Output;
            ConversionStepDescriptions.Clear();
            foreach (var step in result.Steps)
                ConversionStepDescriptions.Add($"{step.Description}  →  {step.Detail}");
            StatusLine = $"Converted {ConvertInput} (base {ConvertFromBase}) = {result.Output} (base {ConvertToBase}). Decimal value: {result.Value}.";
        }
        catch (Exception ex)
        {
            ConvertOutput = string.Empty;
            ConversionStepDescriptions.Clear();
            StatusLine = $"Conversion failed: {ex.Message}";
            _logger.Warn($"Base conversion failed: {ex.Message}");
        }
    }

    private void ComputeTwosComplement()
    {
        try
        {
            long value = long.Parse(TwosComplementInput, CultureInfo.InvariantCulture);
            TwosComplementOutput = BaseConverter.ToTwosComplement(value, TwosComplementBits);
            StatusLine = $"{value} in {TwosComplementBits}-bit two's complement: {TwosComplementOutput}";
        }
        catch (Exception ex)
        {
            TwosComplementOutput = string.Empty;
            StatusLine = $"Two's complement failed: {ex.Message}";
            _logger.Warn($"Two's complement failed: {ex.Message}");
        }
    }

    private void EncodeHamming()
    {
        try
        {
            var bits = ParseBits(HammingDataBits);
            if (bits.Count != 4) throw new ArgumentException("Hamming(7,4) requires exactly 4 data bits.");
            var result = ErrorCorrectingCodes.EncodeHamming74(bits);
            HammingEncodedBits = string.Join(" ", result.Encoded);
            HammingEncodeSteps.Clear();
            foreach (var s in result.ExplanationSteps) HammingEncodeSteps.Add(s);
            StatusLine = $"Encoded {string.Join(" ", bits)} → {HammingEncodedBits}.";
        }
        catch (Exception ex)
        {
            HammingEncodedBits = string.Empty;
            HammingEncodeSteps.Clear();
            StatusLine = $"Encode failed: {ex.Message}";
            _logger.Warn($"Hamming encode failed: {ex.Message}");
        }
    }

    private void DecodeHamming()
    {
        try
        {
            var bits = ParseBits(HammingReceivedBits);
            if (bits.Count != 7) throw new ArgumentException("Hamming(7,4) requires exactly 7 received bits.");
            var result = ErrorCorrectingCodes.DecodeHamming74(bits);
            HammingDecodedData = string.Join(" ", result.Data);
            HammingErrorReport = result.ErrorPosition.HasValue
                ? $"Single-bit error detected and corrected at position {result.ErrorPosition.Value}."
                : "No errors detected.";
            HammingDecodeSteps.Clear();
            foreach (var s in result.ExplanationSteps) HammingDecodeSteps.Add(s);
            StatusLine = $"Decoded {string.Join(" ", bits)}: {HammingErrorReport}";
        }
        catch (Exception ex)
        {
            HammingDecodedData = string.Empty;
            HammingErrorReport = string.Empty;
            HammingDecodeSteps.Clear();
            StatusLine = $"Decode failed: {ex.Message}";
            _logger.Warn($"Hamming decode failed: {ex.Message}");
        }
    }

    private static List<int> ParseBits(string text)
    {
        var bits = new List<int>();
        if (string.IsNullOrWhiteSpace(text)) return bits;
        foreach (char c in text)
        {
            if (c == '0') bits.Add(0);
            else if (c == '1') bits.Add(1);
            else if (!char.IsWhiteSpace(c) && c != ',')
                throw new FormatException($"Invalid bit character: '{c}'.");
        }
        return bits;
    }
}
