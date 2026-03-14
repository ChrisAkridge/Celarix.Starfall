using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;
using Celarix.Starfall.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class TransformInBothBuilder
    {
        private enum TransformType
        {
            Bounds,
            Basic
        }

        private readonly List<QueriedTransformForPersisting> queriedTransforms = [];
        private QueriedTransformForPersisting? currentTransformBuilding;
        private TransformType? currentTransformType;

        public TransformInBothBuilder ForClass(string className)
        {
            CompleteCurrentTransformBuilding();
            var query = SelectionQuery<HeliumRenderable>.StartByClass(className);
            currentTransformBuilding = new QueriedTransformForPersisting
            {
                Query = query
            };
            return this;
        }

        public TransformInBothBuilder ForId(string id)
        {
            CompleteCurrentTransformBuilding();
            var query = SelectionQuery<HeliumRenderable>.StartById(id);
            currentTransformBuilding = new QueriedTransformForPersisting
            {
                Query = query
            };
            return this;
        }

        public TransformInBothBuilder ForAll()
        {
            CompleteCurrentTransformBuilding();
            currentTransformBuilding = new QueriedTransformForPersisting
            {
                Query = null
            };
            return this;
        }

        public TransformInBothBuilder AnimateBounds(Easing easing)
        {
            ThrowIfNoCurrentTransformBuilding();
            SetCurrentTransformTypeOrThrow(TransformType.Bounds);
            currentTransformBuilding!.Transform = new AnimateBoundsTransform(easing);
            return this;
        }

        public TransformInBothBuilder AnimateBasic(BasicTransform transform)
        {
            ThrowIfNoCurrentTransformBuilding();
            SetCurrentTransformTypeOrThrow(TransformType.Basic);
            currentTransformBuilding!.Transform = new AnimateBasicPersistingTransform(transform);
            return this;
        }

        public IReadOnlyList<QueriedTransformForPersisting> Build(IReadOnlyList<HeliumRenderable> renderables)
        {
            CompleteCurrentTransformBuilding();
            return [.. queriedTransforms];
        }

        private void CompleteCurrentTransformBuilding()
        {
            if (currentTransformBuilding is not null)
            {
                queriedTransforms.Add(currentTransformBuilding);
                currentTransformBuilding = null;
            }
        }

        private void ThrowIfNoCurrentTransformBuilding()
        {
            if (currentTransformBuilding is null)
            {
                throw new InvalidOperationException("No current transform building. Call ForClass or ForId first.");
            }
        }

        private void SetCurrentTransformTypeOrThrow(TransformType transformType)
        {
            if (currentTransformType is not null && currentTransformType != transformType)
            {
                throw new InvalidOperationException($"This builder is building a {currentTransformType} transform and cannot start a {transformType} transform.");
            }
        }
    }
}
