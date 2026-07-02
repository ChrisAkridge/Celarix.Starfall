using AngleSharp.Attributes;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal sealed class BinaryDrawingExampleElement : AtriaElement
    {
        private const int BytesPerRow = 16;
        private const double ByteMargin = 0.3d;
        private const double BinaryDrawnImageMaxHeightOfElement = 0.9d;
        private const double BoxPaddingRatio = 0.05d;
        private const double TripletPaddingRatio = BoxPaddingRatio;

        private static readonly SColor AddressColor = new(0xBF, 0xBF, 0xBF, 0xFF);
        private static readonly SColor ByteColor = SColor.White;
        private static readonly SColor InitialBoxColor = new(0x7F, 0x7F, 0x7F, 0xCF);
        private static readonly SColor TripletColor = new(0x3F, 0x3F, 0x3F, 0xFF);

        private SSizeF _byteSize;   // Basis size for all other layout calculations
        private SSizeF _addressSize;
        private float _fontSize;
        private SFont _font;
        private byte[] _data;
        private Rgba32[] _pixels;
        private SImage? _binaryDrawnImage;
        private AnimationContext _animationContext = new();

        // Transition fields
        private BinaryDrawingStage _stage;
        private bool _inTransition;
        private BinaryDrawingStage? _fromStage;
        private BinaryDrawingStage? _toStage;
        private double? _transitionProgress;

        // Transitionable fields
        private bool _drawBytes;
        private double _byteYOffset;
        private double _tripletsOpacity;
        private double _boxesOpacity;
        private double _boxesColoringProgress;
        private double _boxesMergeProgress;
        private double _pixelScaleFactor;
        private SPointF _drawnImagePosition;
        private int _drawnImageRows;

        // Backing lists for box drawing computation
        private List<ColoredBox> _initialBoxes = new();
        private List<ColoredBox> _initialTripletBoxes = new();
        private List<ColoredBox> _nonOverlappingBoxes = new();
        private List<ColoredBoxPoint> _boxPoints = new();
        private List<ColoredBox> _boxesSplitIntoRows = new();
        private List<ColoredBox> _tripletsSplitIntoRows = new();
        private List<ColoredBox> _firstRowPixels = new();

        private SSizeF ByteSquare
        {
            get
            {
                //var maxAxis = Math.Max(_byteSize.Width, _byteSize.Height);
                //return new SSizeF(maxAxis, maxAxis);
                return new SSizeF(_byteSize.Width, _byteSize.Width); // turns out "FF" is taller than it is wide, so it threw everything off
            }
        }

        private double ByteMarginActual => _byteSize.Width * ByteMargin;
        private double AddressWidthWithMargin => _addressSize.Width + ByteMarginActual;

        private SSizeF RowSize
        {
            get
            {
                var rowWidth = (ByteSquare.Width * BytesPerRow) + (ByteMarginActual * BytesPerRow);
                var rowHeight = _byteSize.Height + (ByteMarginActual * 2d);
                return new SSizeF(rowWidth + AddressWidthWithMargin, rowHeight);
            }
        }

        public float FontSize
        {
            get => _fontSize;
        }

        public override void Render(IRenderTarget target)
        {
            DrawFirstRowPixels(target);
            DrawTripletsAndBoxes(target);
            DrawBytesAndAddresses(target);
            DrawImage(target);
        }

        public override void Update(double deltaTime)
        {
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
            if (!_inTransition) { return; }
        }

        public void SetFontSize(float newSize, MeasurementService measurementService)
        {
            _fontSize = newSize;
            _font = new SFontFamily("Consolas", _fontSize);
            _byteSize = measurementService.MeasureText("FF", _font);
            _addressSize = measurementService.MeasureText("00000000", _font);
        }

        public void SetDataFromFile(string filePath)
        {
            _data = File.ReadAllBytes(filePath);

            var pixelCountFromData = (int)Math.Ceiling(_data.Length / 3d);
            var imageWidth = (int)Math.Ceiling(Math.Sqrt(pixelCountFromData));
            var imageHeight = (int)Math.Ceiling((double)pixelCountFromData / imageWidth);
            _pixels = new Rgba32[imageWidth * imageHeight];

            for (int p = 0; p < _pixels.Length; p++)
            {
                var byteIndex = p * 3;
                var r = (byteIndex < _data.Length) ? _data[byteIndex] : (byte)0;
                var g = (byteIndex + 1 < _data.Length) ? _data[byteIndex + 1] : (byte)0;
                var b = (byteIndex + 2 < _data.Length) ? _data[byteIndex + 2] : (byte)0;
                _pixels[p] = new Rgba32(r, g, b);
            }

            using var image = new SixLabors.ImageSharp.Image<Rgba32>(imageWidth, imageHeight, SixLabors.ImageSharp.Color.Black);
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < imageHeight; y++)
                {
                    var sourceRow = _pixels.AsSpan(y * imageWidth, imageWidth);
                    var destRow = accessor.GetRowSpan(y);
                    sourceRow.CopyTo(destRow);
                }
            });

            _binaryDrawnImage = SImage.FromSixLaborsImage(image);
        }

        private void DrawBytesAndAddresses(IRenderTarget target)
        {
            if (!_drawBytes) { return; }

            var byteXOffset = AlignmentHelper.CenterAlign(0d, Slide!.Size.Width, RowSize.Width);
            var visibleRows = (int)Math.Ceiling((Slide.Size.Height - _byteYOffset) / RowSize.Height);
            for (var i = 0; i < visibleRows; i++)
            {
                var rowYOffset = GetRowYOffset(i);
                var rowStartIndex = i * BytesPerRow;
                if (rowStartIndex >= _data.Length) { break; }
                // Draw address
                var addressText = $"{rowStartIndex:X8}";
                var addressBounds = new SRectF(byteXOffset, rowYOffset, _addressSize.Width, _addressSize.Height);
                target.DrawText(addressText, _font, addressBounds, AddressColor, SAngle.Zero);
                // Draw bytes
                for (var j = 0; j < BytesPerRow; j++)
                {
                    var byteIndex = rowStartIndex + j;
                    if (byteIndex >= _data.Length) { break; }
                    var byteValue = _data[byteIndex];
                    var byteText = $"{byteValue:X2}";
                    var byteXPosition = GetByteXPosition(j);
                    var byteBounds = new SRectF(byteXPosition, rowYOffset, _byteSize.Width, _byteSize.Height);
                    target.DrawText(byteText, _font, byteBounds, ByteColor, SAngle.Zero);
                }
            }
        }

        private void DrawTripletsAndBoxes(IRenderTarget target)
        {
            if (_tripletsOpacity == 0d && _boxesOpacity == 0d) { return; }

            // Okay, this one's a bit tricky.
            var visibleRows = (int)Math.Ceiling((Slide!.Size.Height - _byteYOffset) / RowSize.Height);
            var visibleBytes = visibleRows * BytesPerRow;

            // Imagine all the boxes for all the bytes are just in one big row that extends to the left.
            // Then imagine drawing boxes behind the bytes, and then triplets behind the boxes. Let's get
            // some more locals done first.
            var boxSize = ByteSquare * (1 + BoxPaddingRatio);
            
            // For a given byte, its box is centered at the byte's center... unless _boxesMergeProgress
            // is > 0, in which case the boxes are moving toward the triplet centers. Let's compute how
            // much each box has moved. Only the first and third boxes in each triplet move.
            var byte0Center = GetByteXPosition(0) + (ByteSquare.Width / 2d);
            var byte1Center = GetByteXPosition(1) + (ByteSquare.Width / 2d);
            double totalByteWidth = byte1Center - byte0Center;
            var boxMovementDistance = totalByteWidth;
            var boxMovement = boxMovementDistance * _boxesMergeProgress;
            var firstByteXDelta = boxMovement;
            var thirdByteXDelta = -boxMovement;
            _initialBoxes.Clear();
            _initialTripletBoxes.Clear();

            // Now we can fill in the boxes for all visible bytes.
            for (var i = 0; i < visibleBytes; i++)
            {
                var boxX = i * totalByteWidth;
                if (i % 3 == 0) { boxX += firstByteXDelta; }
                else if (i % 3 == 2) { boxX += thirdByteXDelta; }

                var byteValue = _data[i];
                SColor byteColor = (i % 3) switch
                {
                    0 => new SColor(byteValue, 0, 0, 255),
                    1 => new SColor(0, byteValue, 0, 255),
                    2 => new SColor(0, 0, byteValue, 255),
                    _ => throw new InvalidOperationException("Unreachable."),
                };
                var actualBoxColor = MathHelpers.InterpolateColor(InitialBoxColor, byteColor, _boxesColoringProgress);
                _initialBoxes.Add(new ColoredBox(boxSize.At(new SPointF(boxX, 0d)), actualBoxColor));
            }

            // But we do want to show overlapping boxes as mixed color rectangles.
            BoxOverlapAlgorithm(boxSize);

            // Okay, cool, but they're all still in one big row. Let's split them up where they need to go.
            var rowWidthBytesOnly = RowSize.Width - AddressWidthWithMargin;
            SplitBoxesIntoRows(_nonOverlappingBoxes, _boxesSplitIntoRows, rowWidthBytesOnly, RowSize.Height);

            // Almost done - they're too far to the left and too far up. We need to adjust for the address
            // and the Y offset of the bytes.
            var boxXOffset = GetByteXPosition(0);
            var boxYOffset = _byteYOffset;

            // Alright, now time for triplets. We will go back to the "bytes as one big row" first as
            // we can reuse SplitBoxesIntoRows.
            var visibleTriplets = (int)Math.Ceiling(visibleBytes / 3d);
            var tripletSize = new SSizeF((totalByteWidth * 3d) - ByteMarginActual, ByteSquare.Height);
            for (var i = 0; i < visibleTriplets; i++)
            {
                var tripletX = (i * tripletSize.Width) + (i * ByteMarginActual);

                _initialTripletBoxes.Add(new ColoredBox(tripletSize.At(new SPointF(tripletX, 0d)), TripletColor));
            }
            SplitBoxesIntoRows(_initialTripletBoxes, _tripletsSplitIntoRows, rowWidthBytesOnly, RowSize.Height);

            // Nice! Now we can draw the triplets.
            foreach (var triplet in _tripletsSplitIntoRows)
            {
                var adjustedTriplet = new SRectF(triplet.Rectangle.X + boxXOffset, triplet.Rectangle.Y + boxYOffset, triplet.Rectangle.Width, triplet.Rectangle.Height);
                adjustedTriplet = adjustedTriplet.ExpandByFactor(TripletPaddingRatio, TripletPaddingRatio);
                target.DrawRectangle(adjustedTriplet, triplet.Color.WithOpacity(_tripletsOpacity), SPaintStyle.Fill, SAngle.Zero);
            }

            // NOW we can draw the boxes.
            foreach (var box in _boxesSplitIntoRows)
            {
                var adjustedBox = new SRectF(box.Rectangle.X + boxXOffset, box.Rectangle.Y + boxYOffset, box.Rectangle.Width, box.Rectangle.Height);
                target.DrawRectangle(adjustedBox, box.Color.WithOpacity(_boxesOpacity), SPaintStyle.Fill, SAngle.Zero);
            }
        }

        private void DrawFirstRowPixels(IRenderTarget target)
        {
            foreach (var pixel in _firstRowPixels)
            {
                target.DrawRectangle(pixel.Rectangle, pixel.Color, SPaintStyle.Fill, SAngle.Zero);
            }
        }

        private void DrawImage(IRenderTarget target)
        {
            if (_drawnImageRows == 0) { return; }

            var imageWidth = _binaryDrawnImage!.Width;
            var sourceRect = new SRectF(0d, 0d, imageWidth, _drawnImageRows);
            var drawnSize = new SSizeF(imageWidth * _pixelScaleFactor, _drawnImageRows * _pixelScaleFactor);
            var destRect = new SRectF(_drawnImagePosition, drawnSize);
            target.DrawCroppedImage(_binaryDrawnImage, sourceRect, destRect, 1d);
        }

        private double GetByteXPosition(int indexOnRow)
        {
            var byteXOffset = AlignmentHelper.CenterAlign(0d, Slide!.Size.Width, RowSize.Width);
            return byteXOffset
                + AddressWidthWithMargin
                + (_byteSize.Width * indexOnRow)
                + (ByteMarginActual * (indexOnRow + 1));
        }

        private double GetRowYOffset(int rowIndex)
        {
            return _byteYOffset + (rowIndex * RowSize.Height);
        }

        private void BoxOverlapAlgorithm(SSizeF boxSize)
        {
            _nonOverlappingBoxes.Clear();
            _boxPoints.Clear();

            // Remove 0-size input boxes if we happen to have any.
            _initialBoxes.RemoveAll(b => b.Rectangle.Width <= 0d || b.Rectangle.Height <= 0d);

            foreach (var box in _initialBoxes)
            {
                _boxPoints.Add(new ColoredBoxPoint(box.Rectangle.Left, PointKind.Left, box.Color));
                _boxPoints.Add(new ColoredBoxPoint(box.Rectangle.Right, PointKind.Right, box.Color));
            }

            _boxPoints.Sort((a, b) => a.XCoordinate.CompareTo(b.XCoordinate));

            double? currentBoxLeft = null;
            int currentDepth = 0;
            var colorList = new MixingColorList();

            foreach (var point in _boxPoints)
            {
                if (point.Kind == PointKind.Left)
                {
                    if (currentDepth > 0)
                    {
                        var bounds = new SRectF(currentBoxLeft!.Value, 0d, point.XCoordinate - currentBoxLeft!.Value, boxSize.Height);
                        _nonOverlappingBoxes.Add(new ColoredBox(bounds, colorList.MixedColor));
                    }

                    currentBoxLeft = point.XCoordinate;
                    colorList.AddColor(point.Color);
                    currentDepth += 1;
                }
                else if (point.Kind == PointKind.Right)
                {
                    if (currentDepth == 0)
                    {
                        throw new InvalidOperationException("Encountered a rectangle that ended without starting.");
                    }
                    var bounds = new SRectF(currentBoxLeft!.Value, 0d, point.XCoordinate - currentBoxLeft!.Value, boxSize.Height);
                    _nonOverlappingBoxes.Add(new ColoredBox(bounds, colorList.MixedColor));
                    colorList.RemoveColor(point.Color);
                    currentDepth -= 1;
                    if (currentDepth > 0)
                    {
                        currentBoxLeft = point.XCoordinate;
                    }
                    else
                    {
                        currentBoxLeft = null;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Encountered a rectangle point with an invalid kind.");
                }
            }

            // Remove any 0-width boxes that may have been created by overlapping boxes with the same left and right edges.
            _nonOverlappingBoxes.RemoveAll(b => b.Rectangle.Width <= 0d);
        }

        private void SplitBoxesIntoRows(List<ColoredBox> inputBoxes, List<ColoredBox> outputBoxes, double rowWidth, double rowHeight)
        {
            outputBoxes.Clear();

            foreach (var box in inputBoxes)
            {
                var leftRowNumber = (int)Math.Floor(box.Rectangle.Left / rowWidth);
                var rightRowNumber = (int)Math.Floor(box.Rectangle.Right / rowWidth);

                var leftRowY = leftRowNumber * rowHeight;
                var rightRowY = rightRowNumber * rowHeight;

                var leftRowX = box.Rectangle.Left - (leftRowNumber * rowWidth);

                if (leftRowNumber == rightRowNumber)
                {
                    outputBoxes.Add(new ColoredBox(new SRectF(leftRowX, leftRowY, box.Rectangle.Width, box.Rectangle.Height), box.Color));
                    continue;
                }

                var firstRectWidth = rowWidth - leftRowX;
                var firstRectBounds = new SRectF(leftRowX, leftRowY, firstRectWidth, box.Rectangle.Height);
                var secondRectWidth = box.Rectangle.Width - firstRectWidth;
                var secondRectBounds = new SRectF(0d, rightRowY, secondRectWidth, box.Rectangle.Height);
                outputBoxes.Add(new ColoredBox(firstRectBounds, box.Color));
                outputBoxes.Add(new ColoredBox(secondRectBounds, box.Color));
            }
        }

        // Transition methods
        public void ShowBytes()
        {
            _drawBytes = true;
            double initialYOffset = Slide!.Size.Height;
            _byteYOffset = initialYOffset;
            var targetYOffset = initialYOffset / 2d;

            _animationContext.ScheduleAnimation(
                FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
                {
                    _byteYOffset = MathHelpers.Ease(initialYOffset, targetYOffset, p, Easings.Land);
                }, () => _stage = BinaryDrawingStage.ShowBytes)
            );
        }

        public void ShowBoxes()
        {
            _animationContext.ScheduleAnimation(
                FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
                {
                    double easedProgress = MathHelpers.Ease(0d, 1d, p, Easings.Linear);
                    _tripletsOpacity = easedProgress;
                    _boxesOpacity = easedProgress;
                }, () => _stage = BinaryDrawingStage.ShowBoxes)
            );
        }

        public void ColorBoxes()
        {
            _animationContext.ScheduleAnimation(
                FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
                {
                    _boxesColoringProgress = MathHelpers.Ease(0d, 1d, p, Easings.Linear);
                }, () => _stage = BinaryDrawingStage.ColorBoxes)
            );
        }

        public void MergeBoxes()
        {
            _animationContext.ScheduleAnimation(
                FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
                {
                    _boxesMergeProgress = MathHelpers.Ease(0d, 1d, p, Easings.Smoothstep);
                }, () => _stage = BinaryDrawingStage.MergeBoxes)
            );
        }

        public void BuildPixelRow()
        {
            _firstRowPixels.Clear();

            var boxXOffset = GetByteXPosition(0);
            var boxYOffset = _byteYOffset;
            foreach (var box in _boxesSplitIntoRows)
            {
                // This should be fine to work with as we should have a valid set of boxes after the
                // last draw call.
                var adjustedBox = new SRectF(box.Rectangle.X + boxXOffset, box.Rectangle.Y + boxYOffset, box.Rectangle.Width, box.Rectangle.Height);
                _firstRowPixels.Add(new ColoredBox(adjustedBox, box.Color));
            }

            _boxesOpacity = 0d;
            _animationContext.ScheduleAnimation(
                FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
                {
                    _tripletsOpacity = MathHelpers.Ease(1d, 0d, p, Easings.Linear);
                }));

            var targetCenterY = Slide!.Size.Height / 4d;
            var targetTopY = targetCenterY - (ByteSquare.Height / 2d);
            var targetLeftX = Slide!.Size.Width / 10d;

            var animationFactories = new Queue<Func<FixedDurationAnimation>>();
            for (int i = 0; i < _firstRowPixels.Count; i++)
            {
                ColoredBox pixel = _firstRowPixels[i];
                var iCopy = i;
                var initialPosition = pixel.Rectangle.Position;
                var targetPosition = new SPointF(targetLeftX, targetTopY);
                animationFactories.Enqueue(() => FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(0.5d), p =>
                {
                    var easedPosition = MathHelpers.Ease(initialPosition, targetPosition, p, Easings.Smoothstep);
                    var newBounds = _firstRowPixels[iCopy].Rectangle.At(easedPosition);
                    _firstRowPixels[iCopy] = new ColoredBox(newBounds, _firstRowPixels[iCopy].Color);
                }));
                targetLeftX += pixel.Rectangle.Width;
            }
            _animationContext.StaggerAnimations(animationFactories, 1, () =>
            {
                Console.WriteLine("Congratulations, you have wired up the staggered animation system correctly.");
                _stage = BinaryDrawingStage.BuildPixelRow;
            });
        }

        public void FillImage()
        {
            var firstPixel = _firstRowPixels[0];
            _pixelScaleFactor = firstPixel.Rectangle.Width;

            var initialPixelScaleFactor = _pixelScaleFactor;
            var imageWidth = _binaryDrawnImage!.Width;
            var imageInitialX = firstPixel.Rectangle.X;
            var imageTargetX = AlignmentHelper.CenterAlign(0d, Slide!.Size.Width, imageWidth);

            var targetCenterY = Slide!.Size.Height / 4d;
            var targetTopY = targetCenterY - (ByteSquare.Height / 2d);
            _drawnImagePosition = new SPointF(imageInitialX, targetTopY);

            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
            {
                _pixelScaleFactor = MathHelpers.Ease(initialPixelScaleFactor, 1d, p, Easings.Smoothstep);
                _drawnImagePosition = new SPointF(MathHelpers.Ease(imageInitialX, imageTargetX, p, Easings.Smoothstep), targetTopY);
            }, ScheduleUncrop));

            _firstRowPixels.Clear();
            _drawnImageRows = 1;
        }

        private void ScheduleUncrop()
        {
            var initialDrawnImageRows = _drawnImageRows;
            var targetDrawnImageRows = _binaryDrawnImage!.Height;


            var initialByteYOffset = _byteYOffset;
            var targetByteYOffset = Slide!.Size.Height; // Move the bytes off the screen
            _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d), p =>
            {
                _drawnImageRows = (int)MathHelpers.Ease(initialDrawnImageRows, targetDrawnImageRows, p, Easings.Smoothstep);
                _byteYOffset = MathHelpers.Ease(initialByteYOffset, targetByteYOffset, p, Easings.Smoothstep);
            }, () => _stage = BinaryDrawingStage.FillImage));
        }
    }
}
