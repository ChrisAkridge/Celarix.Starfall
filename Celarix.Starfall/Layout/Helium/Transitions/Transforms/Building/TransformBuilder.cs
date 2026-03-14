using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class TransformBuilder
    {
        private readonly HeliumScene from;
        private readonly HeliumScene to;
        private readonly SSizeF maxSize;
        private readonly MeasurementService measurementService;

        private readonly IReadOnlyList<HeliumRenderable> renderablesInBoth;
        private readonly IReadOnlyList<HeliumRenderable> renderablesDeparting;
        private readonly IReadOnlyList<HeliumRenderable> renderablesArriving;

        private TransformInBothBuilder transformInBothBuilder;
        private TransformNotInBothBuilder transformDepartingBuilder;
        private TransformNotInBothBuilder transformArrivingBuilder;

        private TransformBuilder(HeliumScene from, HeliumScene to, SSizeF maxSize,
            MeasurementService measurementService,
            IReadOnlyList<HeliumRenderable> renderablesInBoth,
            IReadOnlyList<HeliumRenderable> renderablesDeparting,
            IReadOnlyList<HeliumRenderable> renderablesArriving)
        {
            this.from = from;
            this.to = to;
            this.maxSize = maxSize;
            this.measurementService = measurementService;
            this.renderablesInBoth = renderablesInBoth;
            this.renderablesDeparting = renderablesDeparting;
            this.renderablesArriving = renderablesArriving;
            this.measurementService = measurementService;

            MakeDefaultTransforms();
        }

        public static TransformBuilder Start(HeliumScene from, HeliumScene to, SSizeF maxSize, MeasurementService measurementService)
        {
            var fromRenderables = from.GetRenderables(maxSize, measurementService).Cast<HeliumRenderable>().ToList();
            var toRenderables = to.GetRenderables(maxSize, measurementService).Cast<HeliumRenderable>().ToList();

            // Renderables are considered the same if they have the same non-empty ID.
            var renderablesInBoth = fromRenderables.Where(fr => !string.IsNullOrEmpty(fr.Id) && toRenderables.Any(tr => tr.Id == fr.Id)).ToList();
            var renderablesDeparting = fromRenderables.Except(renderablesInBoth).ToList();
            var renderablesArriving = toRenderables.Except(renderablesInBoth).ToList();

            return new TransformBuilder(from, to, maxSize, measurementService, renderablesInBoth, renderablesDeparting, renderablesArriving);
        }

        public TransformBuilder WhenInBoth(Action<TransformInBothBuilder> builderAction)
        {
            transformInBothBuilder = new TransformInBothBuilder();
            builderAction(transformInBothBuilder);
            return this;
        }

        public TransformBuilder WhenDeparting(Action<TransformNotInBothBuilder> builderAction)
        {
            transformDepartingBuilder = new TransformNotInBothBuilder();
            builderAction(transformDepartingBuilder);
            return this;
        }

        public TransformBuilder WhenArriving(Action<TransformNotInBothBuilder> builderAction)
        {
            transformArrivingBuilder = new TransformNotInBothBuilder();
            builderAction(transformArrivingBuilder);
            return this;
        }

        public HeliumTransformSet BuildSet()
        {
            return new HeliumTransformSet(from, to, maxSize, measurementService,
                transformInBothBuilder.Build(renderablesInBoth),
                transformDepartingBuilder.Build(renderablesDeparting),
                transformArrivingBuilder.Build(renderablesArriving));
        }

        private void MakeDefaultTransforms()
        {
            transformInBothBuilder = new TransformInBothBuilder();
            transformInBothBuilder.ForAll().AnimateBounds(Easings.Linear);

            transformDepartingBuilder = new TransformNotInBothBuilder();
            transformDepartingBuilder.ForAll().FadeOut(Easings.Linear);

            transformArrivingBuilder = new TransformNotInBothBuilder();
            transformArrivingBuilder.ForAll().FadeIn(Easings.Linear);
        }
    }
}
