using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Layout.Helium.Selection;
using Celarix.Starfall.Mathematics;

namespace Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building
{
    public sealed class TransformNotInBothBuilder
    {
        private enum TransformType
        {
            Bounds,
            Basic
        }

        private readonly List<QueriedTransformForTransients> queriedTransforms = [];
        private QueriedTransformForTransients? currentTransformBuilding;
        private TransformType? currentTransformType;

        public TransformNotInBothBuilder ForClass(string className)
        {
            CompleteCurrentTransformBuilding();
            var query = SelectionQuery<HeliumRenderable>.StartByClass(className);
            currentTransformBuilding = new QueriedTransformForTransients
            {
                Query = query
            };
            return this;
        }

        public TransformNotInBothBuilder ForId(string id)
        {
            CompleteCurrentTransformBuilding();
            var query = SelectionQuery<HeliumRenderable>.StartById(id);
            currentTransformBuilding = new QueriedTransformForTransients
            {
                Query = query
            };
            return this;
        }

        public TransformNotInBothBuilder ForAll()
        {
            CompleteCurrentTransformBuilding();
            currentTransformBuilding = new QueriedTransformForTransients
            {
                Query = null
            };
            return this;
        }

        public TransformNotInBothBuilder FadeIn(Easing easing)
        {
            ThrowIfNoCurrentTransformBuilding();
            SetCurrentTransformTypeOrThrow(TransformType.Basic);
            currentTransformBuilding!.Transform = new FadeTransform(FadeDirection.In, easing);
            return this;
        }

        public TransformNotInBothBuilder FadeOut(Easing easing)
        {
            ThrowIfNoCurrentTransformBuilding();
            SetCurrentTransformTypeOrThrow(TransformType.Basic);
            currentTransformBuilding!.Transform = new FadeTransform(FadeDirection.Out, easing);
            return this;
        }

        public IReadOnlyList<QueriedTransformForTransients> Build(IReadOnlyList<HeliumRenderable> renderables)
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