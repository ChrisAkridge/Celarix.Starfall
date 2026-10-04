using Avalonia.Threading;
using Celarix.Starfall.Atria;
using Celarix.Starfall.Harness.Rendering;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Harness;

/// <summary>
/// Runs one presentation: owns the layout engine and render target, tracks the current slide,
/// and implements Advance / Rewind / jump / reinitialize the same way the console runner does.
/// </summary>
internal sealed class PresentationController : IDisposable
{
    private readonly AtriaLayoutEngine _engine;
    private bool _rewindOccurred;
    private TimeSpan? _lastFrameTimestamp;

    public PresentationController(PresentationDefinition presentation, int viewportWidth, int viewportHeight, IPresenterInput input)
    {
        Presentation = presentation;
        Framebuffer = new AvaloniaFramebuffer(viewportWidth, viewportHeight);
        Target = new SkiaCanvasTarget();
        _engine = new AtriaLayoutEngine(viewportWidth, viewportHeight);
        _engine.Attach(Target);
        _engine.Runtime!.Input = input;
        _engine.OnException += Engine_OnException;
    }

    public PresentationDefinition Presentation { get; }

    public AvaloniaFramebuffer Framebuffer { get; }

    public SkiaCanvasTarget Target { get; }

    public int CurrentSlideIndex { get; private set; } = -1;

    public AtriaSlide? CurrentSlide => _engine.CurrentSlide;

    /// <summary>
    /// Gets or sets whether frames are paused, e.g. while a prompt is open.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Raised whenever the slide or beat changes, so the operator view can refresh.
    /// </summary>
    public event EventHandler? StateChanged;

    public event EventHandler<string>? Log;

    public void Start() => GoToSlide(0);

    public void Advance()
    {
        // A prompt is open (it pauses frames); ignore keys until it closes so Advance can't re-enter.
        if (IsPaused)
        {
            return;
        }

        var result = Run(() => _engine.AdvanceCurrentSlide());
        if (result == SlideAdvanceResult.CanAdvance && CurrentSlideIndex < Presentation.Slides.Count - 1)
        {
            GoToSlide(CurrentSlideIndex + 1);
        }
        else
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// The first press reinitializes the current slide; a second press goes to the previous slide.
    /// Most slides can't reverse their beats, so this matches the console runner.
    /// </summary>
    public void Rewind()
    {
        if (IsPaused)
        {
            return;
        }

        if (!_rewindOccurred)
        {
            ReinitializeCurrentSlide();
            _rewindOccurred = true;
        }
        else
        {
            GoToSlide(Math.Max(CurrentSlideIndex - 1, 0));
        }
    }

    public void ReinitializeCurrentSlide() => GoToSlide(Math.Max(CurrentSlideIndex, 0), resetRewind: false);

    public void GoToSlide(int index) => GoToSlide(index, resetRewind: true);

    /// <summary>
    /// Renders one frame. Call once per display refresh with the refresh timestamp.
    /// </summary>
    public void RenderFrame(TimeSpan timestamp)
    {
        var delta = _lastFrameTimestamp is { } last ? (timestamp - last).TotalSeconds : 1.0 / 60;
        _lastFrameTimestamp = timestamp;
        if (IsPaused)
        {
            return;
        }

        Framebuffer.BeginFrame(Target);
        try
        {
            _engine.OnFrameRequested(Math.Clamp(delta, 0, 0.25));
        }
        finally
        {
            Framebuffer.EndFrame(Target);
        }
    }

    public void Dispose()
    {
        if (_engine.CurrentSlideName is { } name)
        {
            _engine.RemoveSlide(name);
        }
        Target.Dispose();
        Framebuffer.Dispose();
    }

    private void GoToSlide(int index, bool resetRewind)
    {
        if (index < 0 || index >= Presentation.Slides.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (resetRewind)
        {
            _rewindOccurred = false;
        }

        var definition = Presentation.Slides[index];
        if (_engine.CurrentSlideName is { } currentName)
        {
            _engine.RemoveSlide(currentName);
        }

        CurrentSlideIndex = index;
        try
        {
            var slide = definition.Factory(_engine.Runtime!);
            _engine.AddSlide(slide, definition.DisplayName);
            _engine.SetCurrentSlide(definition.DisplayName);
            WriteLog($"Slide {index}: {definition.DisplayName}");
        }
        catch (Exception ex)
        {
            WriteLog($"Could not start slide {index} ({definition.DisplayName}): {ex}");
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private SlideAdvanceResult Run(Func<SlideAdvanceResult> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            Engine_OnException(this, ex);
            return SlideAdvanceResult.InternalStateChanged;
        }
    }

    private void Engine_OnException(object? sender, Exception e)
    {
        WriteLog($"Slide error: {e.Message}{Environment.NewLine}{e}");

        // Same recovery as the console runner: rebuild the current slide on the next UI turn,
        // outside the frame or advance that failed.
        Dispatcher.UIThread.Post(ReinitializeCurrentSlide);
    }

    private void WriteLog(string message) => Log?.Invoke(this, message);
}
