using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements.Containers
{
    public sealed class FlexWrappingContainer : HeliumElement
    {
        private sealed class FlexChildInfo
        {
            public HeliumElement Child { get; }
            public double SizeMultiplier { get; } = 1.0;

            public FlexChildInfo(HeliumElement child, double sizeMultiplier)
            {
                Child = child;
                SizeMultiplier = sizeMultiplier;
            }
        }

        private readonly List<FlexChildInfo> children = [];

        /// <summary>
        /// Gets or sets the baseline height used for layout calculations. This is measured in units
        /// of multiples of this container's height. For example, if this container has a height of 1,
        /// and the BaseLineHeight is set to 0.1, then a line will default to 1/10th of the container's
        /// height. This doesn't necessarily mean exactly 10 lines fit - if you set SizeMultiplier of
        /// a child to something > 1, the line will expand to fit.
        /// </summary>
        public double BaseLineHeight { get; set; }

        public override IReadOnlyList<HeliumElement> Children => children.Select(c => c.Child).ToList();

        public override double DesiredWidthFraction => Constants.FullSize;

        public override double DesiredHeightFraction => Constants.FullSize;

        public override void ArrangeChildren(SRectF thisBounds)
        {
            throw new NotImplementedException();
        }

        public override HeliumElement Clone()
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IRenderable> GetRenderables(MeasurementService measurementService)
        {
            throw new NotImplementedException();
        }

        public override void MeasureSelf(SSizeF availableSize, MeasurementService measurementService)
        {
            // Containers are always as large as they can be.
            ActualSize = availableSize;

            // We don't actually know the available width for each child, though. We do know the height, and
            // the question we want to ask each child is "if you were given the full height, how wide would
            // you want to be?" But all we have is MeasureSelf(SSizeF), which means we have to give them some
            // width. In fact, it gets even worse - many children will always report that they fill their entire
            // available width... which we have as much of as we want. So here's what we'll do - give the child
            // a 1000x1000 area to measure itself in, and see what aspect ratio it wants to be. Oddly, this means
            // children that measure themselves as wide but not very tall will end up being bigger in the actual
            // container.
            var falseSize = new SSizeF(1000, 1000);
            foreach (var flexChild in children)
            {
                var child = flexChild.Child;
                child.MeasureSelf(falseSize, measurementService);
                var aspectRatio = child.ActualSize!.Value.Width / child.ActualSize!.Value.Height;
                var desiredWidth = aspectRatio * availableSize.Height;
                child.MeasureSelf(new SSizeF(desiredWidth, availableSize.Height), measurementService);
            }

            // Okay, technically we will have to resize many of the children once we figure out what
            // line they go on, but we can do that in ArrangeChildren.
        }
    }
}
