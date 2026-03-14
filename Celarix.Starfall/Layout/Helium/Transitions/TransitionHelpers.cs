using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions
{
    public static class TransitionHelpers
    {
        public static (IReadOnlyList<HeliumRenderable> FromRenderables, IReadOnlyList<HeliumRenderable> ToRenderables)
            GetTransitionRenderables(HeliumScene fromScene, HeliumScene toScene, SSizeF maxSize, MeasurementService measurementService)
        {
            var fromRenderables = new List<HeliumRenderable>();
            var toRenderables = new List<HeliumRenderable>();

            if (fromScene.Root != null)
            {
                fromScene.Root.MeasureSelf(maxSize, measurementService);
                fromScene.Root.ArrangeChildren(new SRectF(SPointF.Zero, maxSize));
                fromRenderables = fromScene.Root
                    .GetRenderables()
                    .Cast<HeliumRenderable>()
                    .ToList();
            }

            if (toScene.Root != null)
            {
                toScene.Root.MeasureSelf(maxSize, measurementService);
                toScene.Root.ArrangeChildren(new SRectF(SPointF.Zero, maxSize));
                toRenderables = toScene.Root
                    .GetRenderables()
                    .Cast<HeliumRenderable>()
                    .ToList();
            }

            return (fromRenderables, toRenderables);
        }

        public static IReadOnlyList<HeliumRenderable> MatchRenderablesById(IReadOnlyList<HeliumRenderable> fromRenderables, IReadOnlyList<HeliumRenderable> toRenderables)
        {
            var matchedRenderables = new List<HeliumRenderable>();
            var toRenderablesById = toRenderables
                .Where(r => !string.IsNullOrEmpty(r.Id))
                .ToDictionary(r => r.Id!, r => r);
            foreach (var fromRenderable in fromRenderables)
            {
                if (string.IsNullOrEmpty(fromRenderable.Id))
                {
                    continue;
                }
                if (toRenderablesById.TryGetValue(fromRenderable.Id!, out var matchingToRenderable))
                {
                    matchedRenderables.Add(matchingToRenderable);
                }
            }
            return matchedRenderables;
        }
    }
}
