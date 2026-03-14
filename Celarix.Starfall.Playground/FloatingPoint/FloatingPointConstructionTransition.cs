using Celarix.Starfall.Layout;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Layout.Helium.Transitions.Transforms;
using Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.FloatingPoint
{
    public sealed class FloatingPointConstructionTransition : IHeliumTransition
    {
        private readonly HeliumTransformSet transform;
        private readonly SColor backgroundColor;

        public double Duration { get; init; }

        public FloatingPointConstructionTransition(double duration,
            HeliumScene from,
            HeliumScene to,
            SSizeF maxSize,
            MeasurementService measurementService)
        {
            Duration = duration;
            transform = TransformBuilder.Start(from, to, maxSize, measurementService)
                .WhenInBoth(builder =>
                {
                    // NOTES:
                    // - Queries select on both "from" and "to" renderables, so there's the following behavior:
                    //  - Renderable on both: element transforms from "from" to "to"
                    //  - Renderable only on "from": element is departing. Any builder calls below are ignored
                    //    and whatever is specified in the Departures call is used instead.
                    //  - Renderable only on "to": element is arriving. Any builder calls below are ignored
                    //    and whatever is specified in the Arrivals call is used instead.
                    // - Lots of defaults. For instance, the Arrivals and Departures calls down there are
                    //   already in their default state. Nothing is left unspecified.
                    // - Calling a builder method twice overwrites the previous call. So if you call AnimateBounds and then Basic,
                    //   only the Basic will be used... UNLESS the second method is built to be composable with the first.
                    //   For example, *.AnimateBounds(...).AnimateOpacity(...) would work, but *.AnimateBounds(...).Basic(...) would not,
                    //   because Basic is not composable with AnimateBounds. MAYBE we need to clarify what's composable? Maybe make
                    //   it AnimateBasic and anything starting with Animate starts a new animation that overwrites the previous?
                    // - The builder in WhenInBoth/WhenArriving/WhenDeparting should actually be of two types. The first has a
                    //   bunch of methods for transforming one renderable into another, but the second is just for transforming
                    //   a single renderable. For example:
                    //    - WhenInBoth: ForClass("x").AnimateBounds(...).ForClass("y").AnimateOpacity(...)
                    //    - WhenArriving/WhenDeparting: ForClass("x").AnimatePosition(SomeEnumOrDelegateOrSomething.NearestEdge)
                    //      or AnimatePosition(SPointF)
                    //   Basically, a different API where you get to specify the ending state.

                    builder.ForClass("placeValue")
                        .AnimateBounds(Easings.Smoothstep);
                    builder.ForClass("digit")
                        .AnimateBounds(Easings.Smoothstep);
                    builder.ForId("window")
                        .AnimateBounds(Easings.Smoothstep);
                    builder.ForId("arrow")
                        .AnimateBounds(Easings.Smoothstep);
                    builder.ForClass("advanced")
                        .AnimateBasic(BasicTransforms.StepStart);
                })
                .WhenArriving(builder => builder.ForAll().FadeIn(Easings.Smoothstep))
                .WhenDeparting(builder => builder.ForAll().FadeOut(Easings.Smoothstep))
                .BuildSet();
            backgroundColor = from.BackgroundColor;
        }

        public void Render(double progress, IRenderTarget renderTarget)
        {
            renderTarget.Clear(backgroundColor);
            var renderables = transform.ApplyAll(progress);
            foreach (var renderable in renderables)
            {
                renderable.Render(renderTarget);
            }
        }
    }
}
