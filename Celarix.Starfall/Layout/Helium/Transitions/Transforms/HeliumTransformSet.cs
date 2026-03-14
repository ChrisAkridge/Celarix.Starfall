using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms
{
    public sealed class HeliumTransformSet
    {
        private readonly HeliumScene from;
        private readonly HeliumScene to;
        private readonly SSizeF maxSize;
        private readonly MeasurementService measurementService;
        private readonly IReadOnlyList<QueriedTransformForPersisting> persistingTransforms;
        private readonly IReadOnlyList<QueriedTransformForTransients> departingTransforms;
        private readonly IReadOnlyList<QueriedTransformForTransients> arrivingTransforms;

        internal HeliumTransformSet(HeliumScene from,
            HeliumScene to,
            SSizeF maxSize,
            MeasurementService measurementService,
            IReadOnlyList<QueriedTransformForPersisting> persistingTransforms,
            IReadOnlyList<QueriedTransformForTransients> departingTransforms,
            IReadOnlyList<QueriedTransformForTransients> arrivingTransforms)
        {
            this.persistingTransforms = persistingTransforms;
            this.departingTransforms = departingTransforms;
            this.arrivingTransforms = arrivingTransforms;
            this.from = from;
            this.to = to;
            this.maxSize = maxSize;
            this.measurementService = measurementService;
        }

        public IReadOnlyList<IRenderable> ApplyAll(double progress)
        {
            var fromRenderables = from.GetRenderables(maxSize, measurementService).Cast<HeliumRenderable>().ToList();
            var toRenderables = to.GetRenderables(maxSize, measurementService).Cast<HeliumRenderable>().ToList();

            var querylessDepartingTransforms = departingTransforms.Where(dt => dt.Query == null).ToList();
            var querylessArrivingTransforms = arrivingTransforms.Where(at => at.Query == null).ToList();

            // There's a few different groups and we handle them differently:
            //  - Unidentified elements in "from": We have no way of knowing if these are the same as
            //    any elements in "to", so we just treat them as departing but only those transforms
            //    with no query (i.e. that apply to all departing elements) are applied to them.
            var unidentifiedFromRenderables = querylessDepartingTransforms.Count > 0
                ? fromRenderables.Where(fr => string.IsNullOrEmpty(fr.Id)).ToList()
                : [];   // but just hide them all immediately if there aren't any queryless departing transforms
            ApplyForDeparting(unidentifiedFromRenderables, querylessDepartingTransforms, progress);
            //  - Unidentified elements in "to": Same deal but for arriving.
            var unidentifiedToRenderables = querylessArrivingTransforms.Count > 0
                ? toRenderables.Where(tr => string.IsNullOrEmpty(tr.Id)).ToList()
                : [];   // but just don't show them until the end if there aren't any queryless arriving transforms
            ApplyForArrivals(unidentifiedToRenderables, querylessArrivingTransforms, progress);
            //  - Identified elements: This group splits into three...
            var identifiedFromRenderables = fromRenderables.Where(fr => !string.IsNullOrEmpty(fr.Id)).ToList();
            var identifiedToRenderables = toRenderables.Where(tr => !string.IsNullOrEmpty(tr.Id)).ToList();
            //      - Persisting: If an element matches its ID between "from" and "to", it's considered
            //        the same element persisting through the transition, and we apply the persisting transforms to it.
            var persistingRenderables = identifiedFromRenderables.Where(fr => identifiedToRenderables.Any(tr => tr.Id == fr.Id)).ToList();
            //      - Departing: If an element in "from" doesn't have a match in "to", it's considered
            //        departing, and we apply the departing transforms to it.
            var departingRenderables = identifiedFromRenderables.Where(fr => !persistingRenderables.Any(pr => pr.Id == fr.Id)).ToList();
            //      - Arriving: If an element in "to" doesn't have a match in "from", it's considered
            //        arriving, and we apply the arriving transforms to it.
            var arrivingRenderables = identifiedToRenderables.Where(tr => !persistingRenderables.Any(pr => pr.Id == tr.Id)).ToList();
            ApplyForPersisting(progress, toRenderables, persistingRenderables);
            ApplyForDeparting(departingRenderables, departingTransforms, progress);
            ApplyForArrivals(arrivingRenderables, arrivingTransforms, progress);

            return unidentifiedFromRenderables
                .Concat(unidentifiedToRenderables)
                .Concat(persistingRenderables)
                .Concat(departingRenderables)
                .Concat(arrivingRenderables)
                .ToList();
        }

        private void ApplyForPersisting(double progress, List<HeliumRenderable> toRenderables, List<HeliumRenderable> persistingRenderables)
        {
            foreach (var persistingTransform in persistingTransforms)
            {
                var applicableRenderables = persistingTransform.Query != null
                    ? persistingTransform.Query.Query(persistingRenderables)
                    : persistingRenderables;
                foreach (var renderable in applicableRenderables)
                {
                    var matchingToRenderable = toRenderables.First(tr => tr.Id == renderable.Id);
                    persistingTransform.Transform?.Apply(renderable, matchingToRenderable, progress);
                }
            }
        }

        private void ApplyForDeparting(List<HeliumRenderable> departingRenderables,
            IReadOnlyList<QueriedTransformForTransients> departingTransforms,
            double progress)
        {
            foreach (var departingTransform in departingTransforms)
            {
                var applicableRenderables = departingTransform.Query != null
                    ? departingTransform.Query.Query(departingRenderables)
                    : departingRenderables;
                foreach (var renderable in applicableRenderables)
                {
                    departingTransform.Transform?.Apply(renderable, progress);
                }
            }
        }

        private void ApplyForArrivals(List<HeliumRenderable> arrivingRenderables,
            IReadOnlyList<QueriedTransformForTransients> arrivingTransforms,
            double progress)
        {
            foreach (var arrivingTransform in arrivingTransforms)
            {
                var applicableRenderables = arrivingTransform.Query != null
                    ? arrivingTransform.Query.Query(arrivingRenderables)
                    : arrivingRenderables;
                foreach (var renderable in applicableRenderables)
                {
                    arrivingTransform.Transform?.Apply(renderable, progress);
                }
            }
        }
    }
}
