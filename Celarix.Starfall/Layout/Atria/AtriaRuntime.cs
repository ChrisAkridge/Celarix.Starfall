using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Layout.Atria;

/// <summary>
/// Engine-scoped services that are required to construct and run Atria slides.
/// </summary>
public sealed class AtriaRuntime
{
    public MeasurementService MeasurementService { get; }
    public DebugMode DebugMode { get; }
    public AnimationContextRegistry AnimationContexts { get; }
    public SSizeF ViewportSize { get; }

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
