using DiscreteMathToolkit.Core.Algorithms;

namespace DiscreteMathToolkit.Visualization.Playback;

/// <summary>
/// Framework-agnostic playback model for any sequence of <see cref="AlgorithmStep{TState}"/>.
/// The WPF UI in Gen 2 binds buttons and a timer to this controller.
/// </summary>
public sealed class StepController<TState>
{
    private readonly IReadOnlyList<AlgorithmStep<TState>> _steps;
    private int _index;

    /// <summary>Step delay in milliseconds during auto-play (clamped to [50, 5000]).</summary>
    public int DelayMs { get; private set; } = 700;
    public bool IsPlaying { get; private set; }

    public StepController(IReadOnlyList<AlgorithmStep<TState>> steps)
    {
        _steps = steps;
        _index = 0;
    }

    public AlgorithmStep<TState> Current => _steps[_index];
    public int Index => _index;
    public int TotalSteps => _steps.Count;
    public bool AtStart => _index == 0;
    public bool AtEnd => _index >= _steps.Count - 1;

    public event Action<AlgorithmStep<TState>>? StepChanged;
    public event Action<bool>? PlayingChanged;

    public void Start()
    {
        _index = 0;
        StepChanged?.Invoke(Current);
    }

    public bool Next()
    {
        if (AtEnd) return false;
        _index++;
        StepChanged?.Invoke(Current);
        return true;
    }

    public bool Previous()
    {
        if (AtStart) return false;
        _index--;
        StepChanged?.Invoke(Current);
        return true;
    }

    public void Reset()
    {
        Pause();
        _index = 0;
        StepChanged?.Invoke(Current);
    }

    public void GoTo(int index)
    {
        if (index < 0 || index >= _steps.Count) throw new ArgumentOutOfRangeException(nameof(index));
        _index = index;
        StepChanged?.Invoke(Current);
    }

    public void Play()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        PlayingChanged?.Invoke(true);
    }

    public void Pause()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        PlayingChanged?.Invoke(false);
    }

    public void TogglePlay()
    {
        if (IsPlaying) Pause(); else Play();
    }

    public void SetSpeed(double multiplier)
    {
        // multiplier > 1 means faster, < 1 means slower
        if (multiplier <= 0) throw new ArgumentException("Speed must be positive.", nameof(multiplier));
        int target = (int)Math.Round(700.0 / multiplier);
        DelayMs = Math.Clamp(target, 50, 5000);
    }
}
