using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Layout.Atria.Elements;

/// <summary>
/// Services and ownership information supplied when an element is added to a slide.
/// </summary>
public sealed class AtriaElementContext
{
    public AtriaRuntime Runtime { get; }
    public AtriaSlide Slide { get; }
    public AnimationContext Animations { get; }

    public MeasurementService MeasurementService => Runtime.MeasurementService;
    public SSizeF ViewportSize => Runtime.ViewportSize;

    internal AtriaElementContext(AtriaRuntime runtime, AtriaSlide slide, AnimationContext animations)
    {
        Runtime = runtime;
        Slide = slide;
        Animations = animations;
    }
}
