using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Atria;

/// <summary>
/// Engine-scoped services that are required to construct and run Atria slides.
/// </summary>
public sealed class AtriaRuntime
{
    public MeasurementService MeasurementService { get; }
    public DebugMode DebugMode { get; }
    public AnimationContextRegistry AnimationContexts { get; }
    public SSizeF ViewportSize { get; }

    /// <summary>
    /// Gets or sets where slides get presenter input from. Defaults to the console; hosts can
    /// replace it with dialogs or recorded answers.
    /// </summary>
    public IPresenterInput Input { get; set; } = new ConsolePresenterInput();

    public AtriaRuntime(MeasurementService measurementService,
        DebugMode debugMode,
        AnimationContextRegistry animationContexts,
        SSizeF viewportSize)
    {
        MeasurementService = measurementService;
        DebugMode = debugMode;
        AnimationContexts = animationContexts;
        ViewportSize = viewportSize;
    }
}
