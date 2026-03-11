using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public sealed class TextElement : ResizableHeliumElement
    {
        /// <summary>
        /// Gets or sets the text to be rendered by this element. Multiple lines can be specified by
        /// including newline characters in the string.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        public SFont Font { get; set; } = new SFontFamily("Arial");
        public SColor Color { get; set; } = SColor.Black;
        public SAngle Rotation { get; set; } = SAngle.Zero;
        public Alignment Alignment { get; set; } = Alignment.Center;

        public override IReadOnlyList<HeliumElement> Children => [];

        // We'll let the user set the desired size on their own. Then this will make a TextRenderable,
        // and that's what will ask the render target how big it can be. The TextRenderable will try to
        // fill the width or the height, depending on which is the shorter dimension, and then scale the
        // other dimension to maintain the aspect ratio of the text.
        //
        // If the user sets Font.Size to a non-null value, that font size will be used; otherwise, the 
        // TextRenderable will compute a font size that allows the text to fit within the bounds, and use that.
        // If the specified font size is too large to fit within the bounds, it will be scaled down to fit.

        public override void ArrangeChildren(SRectF thisBounds)
        {
            // No children, so nothing to arrange.
        }

        public override void MeasureSelf(SSizeF maxSize, MeasurementService measurementService)
        {
            if (Font.Size == null)
            {
                base.MeasureSelf(maxSize, measurementService);
                return;
            }

            ActualSize = maxSize;
        }

        public override HeliumElement Clone()
        {
            return new TextElement
            {
                Text = Text,
                desiredWidthFraction = desiredWidthFraction,
                desiredHeightFraction = desiredHeightFraction,
                Font = Font,
                Color = Color,
                Rotation = Rotation,
                Id = Id
            };
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            return [new TextRenderable
            {
                Text = Text,
                Bounds = ActualBounds!.Value,
                Alignment = Alignment,
                Font = Font,
                Color = Color,
                Rotation = Rotation
            }];
        }
    }
}
