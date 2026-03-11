using Celarix.Starfall.Layout.Helium.Renderables;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    /// <summary>
    /// Represents an element that supports multiple lines of text and line wrapping. This class
    /// will be the main target of future advanced text layout features, such as support for colored
    /// words, and more. If you need something simpler, please use <see cref="TextElement"/> instead.
    /// </summary>
    public sealed class AdvancedTextElement : ResizableHeliumElement
    {
        private readonly struct PositionedTextSpan
        {
            public readonly string Text { get; }
            public readonly SRectF ActualBounds { get; }
            public readonly SColor Color { get; }

            public PositionedTextSpan(string text, SRectF actualBounds, SColor color)
            {
                Text = text;
                ActualBounds = actualBounds;
                Color = color;
            }
        }

        private readonly string[] textLines;
        private PositionedTextSpan[]? positionedTextSpans;

        public SFont Font { get; set; } = new SFontFamily("Arial");
        public SColor Color { get; set; } = SColor.Black;
        public SAngle Rotation { get; set; } = SAngle.Zero;
        public Alignment Alignment { get; set; } = Alignment.Center;
        public double LineSpacingMultiplier { get; set; } = 0.2d;

        public override IReadOnlyList<HeliumElement> Children => [];

        public AdvancedTextElement(string text)
        {
            // Split incoming text on /r/n, /r, and /n
            textLines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        }

        public override void MeasureSelf(SSizeF maxSize, MeasurementService measurementService)
        {
            // Alright.
            // So.
            // TODO: Figure out a better layout algorithm for Helium. We can only figure out the text
            // wrapping if we know the width of the element, but we can't know the width of the element
            // until we're told here in MeasureSelf. So we can't do it until now and have to hackishly
            // store the TextMeasurer until we get here.
            if (positionedTextSpans != null
                && ActualSize.HasValue
                && ActualSize.Value == maxSize)  { return; }
            ActualSize = maxSize;

            var separatedLines = textLines
                .Select(l =>
                {
                    // Split the line into words. Words end at any character in the Unicode "Space Separator" category Zs.
                    // Split on those characters, but also include them in the results as separate "words", so we can preserve spacing.
                    return l.TokenizePreservingSpaces();
                })
                .ToArray();

            var fontSizeSpecified = Font.Size != null;
            var fontSize = Font.Size ?? 12f;
            var initialPositioning = PositionTextForFontSize(separatedLines,
                maxSize,
                fontSize,
                measurementService,
                out var positionedBounds);

            var positionedTextFits = positionedBounds.FitsWithin(maxSize.At(SPointF.Zero));
            if (!positionedTextFits || !fontSizeSpecified)
            {
                // The user wants us to expand the text as far as we can,
                // OR the text doesn't fit, so we need to scale it down to fit.
                var hScale = maxSize.Width / positionedBounds.Width;
                var vScale = maxSize.Height / positionedBounds.Height;
                var scale = Math.Min(hScale, vScale);
                var scaledFontSize = fontSize * (float)scale;
                Font = Font.WithSize(scaledFontSize);
                initialPositioning = PositionTextForFontSize(separatedLines,
                    maxSize,
                    scaledFontSize,
                    measurementService,
                    out positionedBounds);
            }

            var alignmentOffset = AlignmentHelper.Align(Alignment, maxSize.At(SPointF.Zero), positionedBounds.Size);
            positionedTextSpans = [.. initialPositioning
                .Select(span => new PositionedTextSpan(
                    span.Text,
                    new SRectF(
                        span.ActualBounds.Left + alignmentOffset.X,
                        span.ActualBounds.Top + alignmentOffset.Y,
                        span.ActualBounds.Width,
                        span.ActualBounds.Height),
                    span.Color))];
        }

        private List<PositionedTextSpan> PositionTextForFontSize(string[][] separatedLines,
            SSizeF maxSize,
            float fontSize,
            MeasurementService measurementService,
            out SRectF positionedBounds)
        {
            var lineSpacing = fontSize * LineSpacingMultiplier;
            var currentX = 0d;
            var currentY = 0d;
            var rightmostX = 0d;
            var bottommostY = 0d;
            var spans = new List<PositionedTextSpan>();

            foreach (var separatedLine in separatedLines)
            {
                foreach (var word in separatedLine)
                {
                    var wordSize = measurementService.MeasureText(word, Font);
                    if (currentX + wordSize.Width > maxSize.Width)
                    {
                        // Move to the next line
                        currentX = 0f;
                        currentY += wordSize.Height + lineSpacing;
                    }
                    spans.Add(new PositionedTextSpan(word, new SRectF(currentX, currentY, wordSize.Width, wordSize.Height), Color));
                    currentX += wordSize.Width;
                    if (currentX > rightmostX)
                    {
                        rightmostX = currentX;
                    }
                }
                // After each line, move to the next line
                currentX = 0f;
                currentY += fontSize + lineSpacing;
                if (currentY > bottommostY)
                {
                    bottommostY = currentY;
                }
            }

            positionedBounds = new SRectF(0, 0, rightmostX, bottommostY);
            return spans;
        }

        public override void ArrangeChildren(SRectF thisBounds)
        {
            // No children, so nothing to arrange.
        }

        public override HeliumElement Clone()
        {
            return new AdvancedTextElement(string.Join("\n", textLines))
            {
                Font = Font,
                desiredWidthFraction = desiredWidthFraction,
                desiredHeightFraction = desiredHeightFraction,
                Color = Color,
                Rotation = Rotation,
                Alignment = Alignment,
                Id = Id
            };
        }

        public override IReadOnlyList<IRenderable> GetRenderables()
        {
            return positionedTextSpans?.Select(span =>
            {
                var renderable = new TextRenderable
                {
                    Text = span.Text,
                    Bounds = new SRectF(
                    ActualPosition!.Value.X + span.ActualBounds.Left,
                    ActualPosition!.Value.Y + span.ActualBounds.Top,
                    span.ActualBounds.Width,
                    span.ActualBounds.Height),
                    Font = Font,
                    Color = span.Color,
                    Rotation = Rotation,
                    Alignment = Alignment,
                    DrawDirectly = true
                };
                renderable.AddClass("advanced");
                return renderable;
            }).ToArray() ?? Array.Empty<IRenderable>();
        }
    }
}
